using System;
using System.Net.Sockets;
using log4net;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;
using Rooms.Protocol.Sockets;

namespace Rooms.Protocol
{

    /// <summary>
    /// Базовый клиент
    /// </summary>
    public class RoomClient : BasePoolChecker, IRoomClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomClient));

        private readonly IBaseSocketClient _baseSocketClient;
        private readonly IPool<IRoomCommand> _poolCommand;
        private readonly IProtocolParser _protocol;

        public RoomClient(IBaseSocketClient socketClient, IPool<byte[]> bytesPool, IPool<IRoomCommand> poolCommandPool,
            IProtocolParser protocol)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            if (poolCommandPool == null)
                throw new ArgumentNullException(nameof(poolCommandPool));

            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            if (socketClient == null)
                throw new ArgumentNullException(nameof(socketClient));

            _protocol = protocol;
            _poolCommand = poolCommandPool;

            _baseSocketClient = socketClient;
            _baseSocketClient.Disconnect += OnDisconnect;
            _baseSocketClient.AfterSend += (s, e) => bytesPool.Free(e.Buffer); //возвращаем данные в пул после отправки, для повторного использования
            _baseSocketClient.AfterReceive += OnAfterReceive;

        }

        private void OnDisconnect(object sender, UnhandledExceptionEventArgs e)
        {
            Disconnect?.Invoke(this, e);
            Detach();
        }

        private void OnAfterReceive(object sender, SocketReceiveEventArgs e)
        {
            var lastIndex = 0;

            //получаем первую команду из буфера приемника
            var command = _protocol.FromBuffer(e.Buffer, lastIndex, e.Length, out lastIndex);
            while (command != null)
            {
                DoCommandProcess(command);

                //продолжаем считывать все команды, пока они не закончаться
                command = _protocol.FromBuffer(e.Buffer, lastIndex, e.Length, out lastIndex);
            }

            //отправляем индекс, на котором остановились
            e.FinishIndex = lastIndex;
        }

        private void DoCommandProcess(IRoomCommand command)
        {
            try
            {
                //возбуждаем событие приема команды
                AfterReceive?.Invoke(this, command);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            finally
            {
                //отправляем команду обратно в пул на переиспользование
                _poolCommand.Free(command);
            }
        }

        /// <summary>
        /// Специальный метод, для логирования исключения
        /// </summary>
        /// <param name="e"></param>
        private void LogException(Exception e)
        {
            Log.Error(e.ToString());
        }


        public void Dispose()
        {
            _baseSocketClient.Dispose();
        }

        public string Room { get; set; }
        public string ClientId { get; set; }

        /// <summary>
        /// Метод, отправляющий команду на сервер
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(IRoomCommand command)
        {
            var data = _protocol.ToBuffer(command);
            _baseSocketClient.SendBytes(data.Item1, data.Item2);
        }

        /// <summary>
        /// Метод, отключающий сокет от сервера 
        /// </summary>
        public void Detach()
        {
            _baseSocketClient.Detach();
            Room = null;
            ClientId = null;
        }

        /// <summary>
        /// Метод, подключающий сокет
        /// </summary>
        /// <param name="socket"></param>
        public void Attach(Socket socket)
        {
            _baseSocketClient.Attach(socket);
        }

        /// <summary>
        /// Свойство, показывающее, что клиент подключен
        /// </summary>
        public bool IsConnected => _baseSocketClient.IsConnected;

        /// <summary>
        /// Событие, возникающее после получения команды
        /// </summary>
        public event Action<IRoomClient, IRoomCommand> AfterReceive;

        /// <summary>
        /// Событие, возникающее при разрыве соединения
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> Disconnect;
    }
}