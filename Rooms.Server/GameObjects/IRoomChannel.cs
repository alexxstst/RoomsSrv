using System;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{

    /// <summary>
    /// Интерфейс комнаты
    /// </summary>
    public interface IRoomChannel: IPoolChecker
    {
        /// <summary>
        /// Идентификатор комнаты
        /// </summary>
        string RoomId { get; set; }

        /// <summary>
        /// Свойство возвращает всех клиентов собравшихся в комнате
        /// </summary>
        IRemoteClient[] Clients { get; }

        /// <summary>
        /// Время последней рассылки сообщений клиентам
        /// </summary>
        DateTime LastClientAccess { get; set; }

        /// <summary>
        /// Свойство, показывающее, что комната уже не активна
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Свойство, показывающее, что комната пустая
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Метод, добавляем клиента в комнату
        /// </summary>
        /// <param name="client"></param>
        void Add(IRemoteClient client);

        /// <summary>
        /// Метод, удаляет клиента из комнаты
        /// </summary>
        /// <param name="client"></param>
        void Remove(IRemoteClient client);

        /// <summary>
        /// Метод, отправляем сообщение всем клиентам, кроме тех, что отсеит фильтер
        /// </summary>
        /// <param name="command"></param>
        /// <param name="filter"></param>
        void SendAll(IRoomCommand command, Func<IRemoteClient, bool> filter);
    }
}