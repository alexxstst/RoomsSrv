using Rooms.Protocol;

namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// Расширенный интерфейс клиента для хранения идентификатора комнаты
    /// </summary>
    public interface IRemoteClient : IRoomClient
    {
        /// <summary>
        /// Метод, подключающий клиента к комнате
        /// </summary>
        /// <param name="roomChannel"></param>
        void AttachToRoom(IRoomChannel roomChannel);

        /// <summary>
        /// Комната, в которой находиться клиент
        /// </summary>
        IRoomChannel RoomChannel { get; }
    }
}