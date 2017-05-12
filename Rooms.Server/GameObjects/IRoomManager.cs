namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// Интерфейс менеджера комнат
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// Список всех созданных комнат
        /// </summary>
        IRoomChannel[] Rooms { get; }

        /// <summary>
        /// Метод, возвращает комнату по её идентификатору
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        IRoomChannel GetRoom(string roomId);

        /// <summary>
        /// Метод, удаляет комнату по её идентификатору
        /// </summary>
        /// <param name="roomChannel"></param>
        /// <param name="reason"></param>
        void FreeRoom(IRoomChannel roomChannel, RoomRemoveReason reason);

        /// <summary>
        /// Метод, присоеденяет клиента к общему потоку обработки данных
        /// </summary>
        /// <param name="remotClient"></param>
        void AttachClient(IRemoteClient remotClient);

        /// <summary>
        /// Свойство, возвращающее объект, собирающий статистику
        /// </summary>
        IRoomManagerStatistics Statistics { get; }
    }

    public enum RoomRemoveReason
    {
        Timeout,
        DisconnectAllClients,
        ServerStop
    }
}