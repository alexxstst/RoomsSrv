using System;
using System.Net.Sockets;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{

    /// <summary>
    /// Базовый клиент для сервера. Имеет свой ID и номер комнаты
    /// </summary>
    public interface IRoomClient: IPoolChecker, IDisposable
    {
        /// <summary>
        /// Номер комнаты, в которой находиться
        /// </summary>
        string Room { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Метод, отправляющий команду на сервер
        /// </summary>
        /// <param name="command"></param>
        void SendCommand(IRoomCommand command);

        /// <summary>
        /// Метод, отключающий сокет от сервера 
        /// </summary>
        void Detach();

        /// <summary>
        /// Метод, подключающий сокет
        /// </summary>
        /// <param name="socket"></param>
        void Attach(Socket socket);

        /// <summary>
        /// Свойство, показывающее, что клиент подключен
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Событие, возникающее после получения команды
        /// </summary>
        event Action<IRoomClient, IRoomCommand> AfterReceive;

        /// <summary>
        /// Событие, возникающее при разрыве соединения
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> Disconnect;
    }


}