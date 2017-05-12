using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Sockets
{

    /// <summary>
    /// ������� ����� ����������� �������
    /// </summary>
    public class BaseSocketClient : IBaseSocketClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BaseSocketClient));

        private readonly IPool<byte[]> _pool;
        private readonly Queue<Tuple<byte[], int>> _sendQueue = new Queue<Tuple<byte[], int>>(10);
        private Socket _socket;
        private byte[] _readCommandBuffer;
        private int _offset;
        private bool _isSendComplete = true;
        private bool _attached;

        /// <summary>
        /// �������, ����������� ��� ����������� �������
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> Disconnect;

        /// <summary>
        /// ������� ����������� ����� ������ ���������
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> AfterReceive;

        /// <summary>
        /// �������, ����������� ����� �������� ���������
        /// </summary>
        public event EventHandler<SocketSendEventArgs> AfterSend;

        /// <summary>
        /// �����������, ����������� ��� � �������. ������������ ��� ��������� ���� �� ����� ������ � ����� �������� ���������.
        /// </summary>
        /// <param name="bytesPool"></param>
        public BaseSocketClient(IPool<byte[]> bytesPool)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            _pool = bytesPool;
        }

        /// <summary>
        /// �����, ������������� ����� ������� � ������������ �������
        /// </summary>
        /// <param name="socket"></param>
        public void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            //��������� ����� �� �������� �����
            if (!socket.Connected)
                throw new InvalidOperationException("Socket is not connected!");

            lock (this)
            {
                //���������, ����� 2 ���� ��������� ���� �� ������� ���� �����
                if (_attached)
                    throw new InvalidOperationException("Socket is already attached!");

                _attached = true; //������������� ���� �� ������������� � ������
                _readCommandBuffer = _pool.Get(); //�������� ����� �� �����
                _sendQueue.Clear(); //�� ������ ������ ������� ������
                _socket = socket;   //������������� �����
                _socket.Blocking = false; //��������� ����� � ������������� �����
                _socket.NoDelay = true;   //�������� �������� ������

                _offset = 0;  //������������� �������� �� ������ ������ ������
                _isSendComplete = true; //������������� ����, ��������� � ���, ��� �� ������ ������ �� ��������

                DoSocketReceive(); //�������� �������� ������ �� ������
            }
        }

        /// <summary>
        /// �����, ������������ ����� � ����������� ���������� � ��� 
        /// </summary>
        public void Detach()
        {
            Detach(null);
        }

        /// <summary>
        /// �����, ������������ ����� � ����������� ���������� � ��� 
        /// </summary>
        /// <param name="e">����������, ���� ��� ��������� ��� ������ � �������</param>
        public void Detach(Exception e)
        {
            //���������, ��� ���� ����� ��������, ��, ������ �� ������.
            if (!_attached)
                return;

            lock (this)
            {
                //����������� ������� �������� � �����������
                if (!_attached)
                    return;

                _attached = false;

                //������� ������� �������� ������ �� ������.
                //��� ����� �� ����� �����, �� ��� � �� ������ - ����� ������.
                foreach (var tuple in _sendQueue)
                    _pool.Free(tuple.Item1);

                //������� �����
                _socket.Dispose();
                _isSendComplete = true;

                //������� ������ �� �����
                _pool.Free(_readCommandBuffer);
                _readCommandBuffer = null;

                //������� �������
                _sendQueue.Clear();
                _socket = null;
                _offset = 0;

                //�������� ������� ����������� ������
                Disconnect?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
            }
        }

        /// <summary>
        /// ����������� �����, ����������� ��� ����� ����������
        /// </summary>
        /// <param name="e"></param>
        protected void LogException(Exception e)
        {
            Logger.Error(e.ToString());
        }

        /// <summary>
        /// �����, ���������� ������ �� ������
        /// </summary>
        private void DoSocketReceive()
        {
            try
            {
                _socket.BeginReceive(_readCommandBuffer, _offset, _readCommandBuffer.Length - _offset, SocketFlags.None,
                    OnCompleteSocketReceive, _socket);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
            }
        }

        /// <summary>
        /// �����, ��������� ��� ����� ���������� �� ������, ���������� ��� � ����������� �����
        /// </summary>
        /// <param name="e"></param>
        private void DoDisconnect(Exception e)
        {
            if (e != null)
                LogException(e);

            Detach(e);
        }

        /// <summary>
        /// �����, ������������ ��� ���������� ������
        /// </summary>
        /// <param name="ar"></param>
        private void OnCompleteSocketReceive(IAsyncResult ar)
        {
            //���� ����� ��� ���������, �� ������ �������
            if (!_attached || _socket == null)
                return;

            SocketError socketError;
            int length;
            try
            {
                //�������� ���������� ����������� ����
                length = _socket.EndReceive(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }

            //��������� �� ������ ��� ������ ������
            if (socketError != SocketError.Success || length <= 0)
            {
                DoDisconnect(new Exception(socketError.ToString()));
                return;
            }

            _offset += length;//������������� ������� ����� ������

            //���������� ������� � ���������� ��� �� �������
            var receiveEvent = new SocketReceiveEventArgs(this, _readCommandBuffer, _offset);

            try
            {
                AfterReceive?.Invoke(this, receiveEvent);
            }
            catch (Exception e)
            {
                //������� �� ����������
                DoDisconnect(e);    
                return;
            }

            //���������, ������� ������� ��������� �� ������ ������
            if (receiveEvent.FinishIndex > 0)
            {
                //������������� ����� ���������
                _offset = receiveEvent.Length - receiveEvent.FinishIndex;
                for (var i = 0; i < _offset; ++i)
                {
                    //��������� ������ �� ������ ��������
                    _readCommandBuffer[i] = _readCommandBuffer[i + receiveEvent.FinishIndex];
                }
            }

            //��������� ��������� ������ ������ �� ������
            DoSocketReceive();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Detach();
        }

        /// <summary>
        /// �����, ������������ ������ ������. ������ ������������ ����������, ������� ����� ����� ���������� ����������
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void SendBytes(byte[] data, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            //���������, ����� ����� ��� ��������
            if (!_attached)
                throw new InvalidOperationException("Socket is detached!");

            lock (_socket)
            {
                if (_isSendComplete)
                {
                    //���� �� ������ ��� ������������ ������, ��
                    _isSendComplete = false; //���������� �������������� ���� (�� ������������ � DoSocketSend ����� ��������)
                    try
                    {
                        //���������� ������ ��������
                        _socket.BeginSend(data, 0, length, SocketFlags.None, DoSocketSend, data);
                    }
                    catch (Exception e)
                    {
                        DoDisconnect(e);
                    }

                }
                else
                    //���� �� ������ ��� ���� ����� ��������, �� ��������� ������ � ������� ��������
                    _sendQueue.Enqueue(new Tuple<byte[], int>(data, length));
            }
        }

        private void DoSocketSend(IAsyncResult ar)
        {
            SocketError socketError;
            int length;
            try
            {
                //�������� ���������� ������������ ������
                length = _socket.EndSend(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }
            finally
            {
                //�������� �������, ��������������� �� �������� ��������� ������ ������
                AfterSend?.Invoke(this, new SocketSendEventArgs(this, (byte[])ar.AsyncState, _offset));
            }

            //��������� �� ������ �������� ������
            if (socketError != SocketError.Success || length <= 0)
            {
                DoDisconnect(new Exception(socketError.ToString()));
                return;
            }

            lock (_socket)
            {
                //���� ������� �����, �� ���������� ���� � ���, ��� ��������� ������ ���
                _isSendComplete = _sendQueue.Count == 0;
                if (!_isSendComplete)
                {
                    //�������� ������ ������ ������
                    var data = _sendQueue.Dequeue();
                    try
                    {
                        //���������� ������ ������
                        _socket.BeginSend(data.Item1, 0, data.Item2, SocketFlags.None, DoSocketSend, data.Item1);
                    }
                    catch (Exception e)
                    {
                        DoDisconnect(e);
                    }
                }
            }
        }


        /// <summary>
        /// ��������, ������������ ������ �������
        /// </summary>
        public bool IsConnected => _attached && _socket.Connected;
    }
}