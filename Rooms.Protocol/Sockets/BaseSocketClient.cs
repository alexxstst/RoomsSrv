using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Sockets
{

    /// <summary>
    /// Базовый класс управляющей сокетом
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
        /// Событие, возникающие при дисконнекте клиента
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> Disconnect;

        /// <summary>
        /// Событие возникающее после приема сообщения
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> AfterReceive;

        /// <summary>
        /// Событие, возникающее после отправки сообщения
        /// </summary>
        public event EventHandler<SocketSendEventArgs> AfterSend;

        /// <summary>
        /// Конструктор, принимающий пул с байтами. Используется при выделении байт на буфер приема и буфер отправки сообщения.
        /// </summary>
        /// <param name="bytesPool"></param>
        public BaseSocketClient(IPool<byte[]> bytesPool)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            _pool = bytesPool;
        }

        /// <summary>
        /// Метод, прикрепляющий сокет клиента к управляющему объекту
        /// </summary>
        /// <param name="socket"></param>
        public void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            //проверяем сокет на признаки жизни
            if (!socket.Connected)
                throw new InvalidOperationException("Socket is not connected!");

            lock (this)
            {
                //проверяем, чтобы 2 раза нехорошие люди не дергали этот метод
                if (_attached)
                    throw new InvalidOperationException("Socket is already attached!");

                _attached = true; //устанавливаем флаг на присоединение к сокету
                _readCommandBuffer = _pool.Get(); //выделяем буфер на прием
                _sendQueue.Clear(); //на всякий случай очищаем данные
                _socket = socket;   //устанавливаем сокет
                _socket.Blocking = false; //переводим сокет в неблокирующий режим
                _socket.NoDelay = true;   //ускоряем отправку данных

                _offset = 0;  //устанавливаем смещение от начала буфера приема
                _isSendComplete = true; //устанавливаем флаг, говорящий о том, что по сокету ничего не посылали

                DoSocketReceive(); //начинаем операцию чтения на сокете
            }
        }

        /// <summary>
        /// Метод, открепляющий сокет и закрывающий соединения с ним 
        /// </summary>
        public void Detach()
        {
            Detach(null);
        }

        /// <summary>
        /// Метод, открепляющий сокет и закрывающий соединения с ним 
        /// </summary>
        /// <param name="e">Исключение, если оно возникает при работе с сокетом</param>
        public void Detach(Exception e)
        {
            //проверяем, что если сокет отключен, то, ничего не делаем.
            if (!_attached)
                return;

            lock (this)
            {
                //стандартная двойная проверка с блокировкой
                if (!_attached)
                    return;

                _attached = false;

                //очищаем очередь отправки данных на сокете.
                //раз дошло до этого места, то уже и не судьба - сокет сломан.
                foreach (var tuple in _sendQueue)
                    _pool.Free(tuple.Item1);

                //удаляем сокет
                _socket.Dispose();
                _isSendComplete = true;

                //очищаем буффер на прием
                _pool.Free(_readCommandBuffer);
                _readCommandBuffer = null;

                //очищаем очередь
                _sendQueue.Clear();
                _socket = null;
                _offset = 0;

                //вызываем событие дисконнекта сокета
                Disconnect?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
            }
        }

        /// <summary>
        /// Специальный метод, вызвающийся при любом исключении
        /// </summary>
        /// <param name="e"></param>
        protected void LogException(Exception e)
        {
            Logger.Error(e.ToString());
        }

        /// <summary>
        /// Метод, начинающий чтение на сокете
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
        /// Метод, вызвающий при любом исключении на сокете, логирующий его и отключающий сокет
        /// </summary>
        /// <param name="e"></param>
        private void DoDisconnect(Exception e)
        {
            if (e != null)
                LogException(e);

            Detach(e);
        }

        /// <summary>
        /// Метод, вызывающийся при завершении чтения
        /// </summary>
        /// <param name="ar"></param>
        private void OnCompleteSocketReceive(IAsyncResult ar)
        {
            //если сокет уже отключили, то просто выходим
            if (!_attached || _socket == null)
                return;

            SocketError socketError;
            int length;
            try
            {
                //получаем количество прочитанных байт
                length = _socket.EndReceive(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }

            //Проверяем на ошибки при чтении сокета
            if (socketError != SocketError.Success || length <= 0)
            {
                DoDisconnect(new Exception(socketError.ToString()));
                return;
            }

            _offset += length;//устанавливаем текущую длину данных

            //генерируем событие и отправляем его на вычетку
            var receiveEvent = new SocketReceiveEventArgs(this, _readCommandBuffer, _offset);

            try
            {
                AfterReceive?.Invoke(this, receiveEvent);
            }
            catch (Exception e)
            {
                //вычитка не получилась
                DoDisconnect(e);    
                return;
            }

            //Проверяем, сколько удалось прочитать из буфера данных
            if (receiveEvent.FinishIndex > 0)
            {
                //устанавливаем новое семещение
                _offset = receiveEvent.Length - receiveEvent.FinishIndex;
                for (var i = 0; i < _offset; ++i)
                {
                    //переносим данные по новому смещению
                    _readCommandBuffer[i] = _readCommandBuffer[i + receiveEvent.FinishIndex];
                }
            }

            //повторяем процедуру чтения данных на сокете
            DoSocketReceive();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Detach();
        }

        /// <summary>
        /// Метод, отправляющий данные сокету. Данный отправляются асинхронно, поэтому метод сразу возвращает управление
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void SendBytes(byte[] data, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            //проверяем, чтобы сокет был активным
            if (!_attached)
                throw new InvalidOperationException("Socket is detached!");

            lock (_socket)
            {
                if (_isSendComplete)
                {
                    //если на сокете нет отправляемых данных, то
                    _isSendComplete = false; //выставляем соответсвующий флаг (он сбрасывается в DoSocketSend после отправки)
                    try
                    {
                        //отправляем данные напрямую
                        _socket.BeginSend(data, 0, length, SocketFlags.None, DoSocketSend, data);
                    }
                    catch (Exception e)
                    {
                        DoDisconnect(e);
                    }

                }
                else
                    //если на сокете уже есть такая операция, то добавляем данные в очередь ожидания
                    _sendQueue.Enqueue(new Tuple<byte[], int>(data, length));
            }
        }

        private void DoSocketSend(IAsyncResult ar)
        {
            SocketError socketError;
            int length;
            try
            {
                //получаем количество отправленных данных
                length = _socket.EndSend(ar, out socketError);
            }
            catch (Exception e)
            {
                DoDisconnect(e);
                return;
            }
            finally
            {
                //вызываем событие, сигнализирующее об отправки очередной порции данных
                AfterSend?.Invoke(this, new SocketSendEventArgs(this, (byte[])ar.AsyncState, _offset));
            }

            //проверяем на ошибки отправки данных
            if (socketError != SocketError.Success || length <= 0)
            {
                DoDisconnect(new Exception(socketError.ToString()));
                return;
            }

            lock (_socket)
            {
                //если очередь пуста, то выставляем флаг о том, что ожидающих данных нет
                _isSendComplete = _sendQueue.Count == 0;
                if (!_isSendComplete)
                {
                    //получаем первую порцию данных
                    var data = _sendQueue.Dequeue();
                    try
                    {
                        //отправляем порцию данных
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
        /// Свойство, показывающее статус клиента
        /// </summary>
        public bool IsConnected => _attached && _socket.Connected;
    }
}