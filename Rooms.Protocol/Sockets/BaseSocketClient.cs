using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Sockets
{
    public class BaseSocketClient : IBaseSocketClient
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(BaseSocketClient));

        private readonly IPool<byte[]> _pool;
        private readonly Queue<Tuple<byte[], int>> _sendQueue = new Queue<Tuple<byte[], int>>(10);
        private Socket _socket;
        private byte[] _readCommandBuffer;
        private int _offset;
        private bool _isSendComplete = true;
        private bool _attached;

        public event EventHandler<UnhandledExceptionEventArgs> Disconnect;
        public event EventHandler<SocketReceiveEventArgs> AfterReceive;
        public event EventHandler<SocketSendEventArgs> AfterSend;

        public BaseSocketClient(IPool<byte[]> bytesPool)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            _pool = bytesPool;
        }

        public void Attach(Socket socket)
        {
            lock (this)
            {
                if (_attached)
                    throw new InvalidOperationException("Socket is already attached!");

                if (socket == null)
                    throw new ArgumentNullException(nameof(socket));

                if (!socket.Connected)
                    throw new InvalidOperationException("Socket is not connected!");


                _attached = true;
                _readCommandBuffer = _pool.Get();
                _sendQueue.Clear();
                _socket = socket;
                _socket.Blocking = false;
                _socket.NoDelay = true;

                _offset = 0;
                _isSendComplete = true;

                DoSocketReceive();
            }
        }

        public void Detach()
        {
            Detach(null);
        }

        public void Detach(Exception e)
        {
            if (!_attached)
                return;

            lock (this)
            {
                foreach (var tuple in _sendQueue)
                    _pool.Free(tuple.Item1);

                _socket.Dispose();
                _isSendComplete = true;
                _attached = false;
                _pool.Free(_readCommandBuffer);
                _readCommandBuffer = null;
                _sendQueue.Clear();
                _socket = null;
                _offset = 0;

                Disconnect?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
            }
        }

        protected void LogException(Exception e)
        {
            _logger.Error(e.ToString());
        }

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

        private void DoDisconnect(Exception e)
        {
            if (e != null)
                LogException(e);

            Detach(e);
        }

        private void OnCompleteSocketReceive(IAsyncResult ar)
        {
            if (_socket == null)
                return;

            SocketError socketError;
            int length;
            try
            {
                length = _socket.EndReceive(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }

            if (socketError != SocketError.Success || length == 0)
            {
                DoDisconnect(null);
                return;
            }

            if (length > 0)
            {
                _offset += length;

                var receiveEvent = new SocketReceiveEventArgs(this, _readCommandBuffer, _offset);
                AfterReceive?.Invoke(this, receiveEvent);

                if (receiveEvent.ReadedLength > 0)
                {
                    _offset = receiveEvent.Length - receiveEvent.ReadedLength;
                    for (var i = 0; i < _offset; ++i)
                    {
                        _readCommandBuffer[i] = _readCommandBuffer[i + receiveEvent.ReadedLength];
                    }
                }
            }

            DoSocketReceive();
        }

        public void Dispose()
        {
            Detach();
        }

        private void CheckAttached()
        {
            if (!_attached)
                throw new InvalidOperationException("Socket is detached!");
        }

        public void SendBytes(byte[] data, int length)
        {
            CheckAttached();

            lock (_socket)
            {
                if (_isSendComplete)
                {
                    _isSendComplete = false;
                    try
                    {
                        _socket.BeginSend(data, 0, length, SocketFlags.None, DoSocketSend, data);
                    }
                    catch (Exception e)
                    {
                        DoDisconnect(e);
                    }

                }
                else
                    _sendQueue.Enqueue(new Tuple<byte[], int>(data, length));
            }
        }

        private void DoSocketSend(IAsyncResult ar)
        {
            SocketError socketError;
            int length;
            try
            {
                length = _socket.EndSend(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }
            finally
            {
                AfterSend?.Invoke(this, new SocketSendEventArgs(this, (byte[])ar.AsyncState, _offset));
            }

            if (socketError != SocketError.Success || length == 0)
            {
                DoDisconnect(null);
                return;
            }

            lock (_socket)
            {
                _isSendComplete = _sendQueue.Count == 0;
                if (!_isSendComplete)
                {
                    var data = _sendQueue.Dequeue();
                    try
                    {
                        _socket.BeginSend(data.Item1, 0, data.Item2, SocketFlags.None, DoSocketSend, data.Item1);
                    }
                    catch (Exception e)
                    {
                        DoDisconnect(e);
                    }
                }
            }
        }

        public bool IsConnected => _attached && _socket.Connected;
    }
}