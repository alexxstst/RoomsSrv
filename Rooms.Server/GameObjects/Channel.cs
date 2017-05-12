using System;
using System.Collections.Generic;
using log4net;
using log4net.Repository.Hierarchy;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{
    public class Channel : BasePoolChecker, IRoomChannel
    {
        private static ILog Log = LogManager.GetLogger(typeof(Channel));

        private readonly List<IRemoteClient> _clients = new List<IRemoteClient>();

        /// <summary>
        /// Идентификатор комнаты
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// Свойство возвращает всех клиентов собравшихся в комнате
        /// </summary>
        public IRemoteClient[] Clients
        {
            get
            {
                //TODO сделать в виде метода
                lock (_clients)
                    return _clients.ToArray();
            }
        }

        /// <summary>
        /// Время последней рассылки сообщений клиентам
        /// </summary>
        public DateTime LastClientAccess { get; set; }

        /// <summary>
        /// Свойство, показывающее, что комната уже не активна
        /// </summary>
        public bool IsExpired => (DateTime.Now - LastClientAccess).TotalMinutes >= 1.0;

        /// <summary>
        /// Свойство, показывающее, что комната пустая
        /// </summary>
        public bool IsEmpty => _clients.Count == 0;

        /// <summary>
        /// Метод, добавляем клиента в комнату
        /// </summary>
        /// <param name="client"></param>
        public void Add(IRemoteClient client)
        {
            if (client.RoomChannel != null)
                throw new InvalidOperationException();

            lock (_clients)
            {
                _clients.Add(client);
            }

            client.AttachToRoom(this);
        }

        /// <summary>
        /// Метод, удаляет клиента из комнаты
        /// </summary>
        /// <param name="client"></param>
        public void Remove(IRemoteClient client)
        {
            if (client.RoomChannel != this)
                throw new InvalidOperationException();

            lock (_clients)
            {
                _clients.Remove(client);
            }

            client.AttachToRoom(null);
        }

        /// <summary>
        /// Метод, отправляем сообщение всем клиентам, кроме тех, что отсеит фильтер
        /// </summary>
        /// <param name="command"></param>
        /// <param name="filter"></param>
        public void SendAll(IRoomCommand command, Func<IRemoteClient, bool> filter)
        {
            LastClientAccess = DateTime.Now;
            lock (_clients)
            {
                //на foreach не переделывать!!! он тормозит, а метод критичен к производительности.
                for(var i =0; i < _clients.Count; ++i)
                {
                    if (filter(_clients[i]))
                        _clients[i].SendCommand(command);
                }
            }
        }
    }
}