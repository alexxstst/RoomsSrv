using System;
using System.Net.Sockets;

namespace Rooms.Protocol.Sockets
{

    /// <summary>
    /// Ѕазовый интерфейс дл€ класса обслуживающего сокеты
    /// </summary>
    public interface IBaseSocketClient: IDisposable
    {
        /// <summary>
        /// —обытие, возникающие при дисконнекте клиента
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> Disconnect;

        /// <summary>
        /// —обытие возникающее после приема сообщени€
        /// </summary>
        event EventHandler<SocketReceiveEventArgs> AfterReceive;

        /// <summary>
        /// —обытие, возникающее после отправки сообщени€
        /// </summary>
        event EventHandler<SocketSendEventArgs> AfterSend;

        /// <summary>
        /// —войство, показывающее статус клиента
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// ћетод, прикрепл€ющий сокет клиента к управл€ющему объекту
        /// </summary>
        /// <param name="socket"></param>
        void Attach(Socket socket);

        /// <summary>
        /// ћетод, открепл€ющий сокет и закрывающий соединени€ с ним 
        /// </summary>
        void Detach();

        /// <summary>
        /// ћетод, отправл€ющий данные сокету. ƒанный отправл€ютс€ асинхронно, поэтому метод сразу возвращает управление
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        void SendBytes(byte[] data, int length);
    }
}