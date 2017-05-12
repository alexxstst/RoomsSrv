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

        public string Name { get; set; }

        public IRemoteClient[] Clients
        {
            get
            {
                lock (_clients)
                    return _clients.ToArray();
            }
        }

        public DateTime LastClientAccess { get; set; }
        public bool IsExpired => (DateTime.Now - LastClientAccess).TotalSeconds >= 10;
        public bool IsEmpty => _clients.Count == 0;

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

        public void SendAll(IRoomCommand command, Func<IRemoteClient, bool> filter)
        {
            LastClientAccess = DateTime.Now;
            lock (_clients)
            {
                for(var i =0; i < _clients.Count; ++i)
                {
                    if (filter(_clients[i]))
                        _clients[i].SendCommand(command);
                }
            }
        }
    }
}