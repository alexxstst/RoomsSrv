using System;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{
    public interface IRoomChannel: IPoolChecker
    {
        string Name { get; set; }
        IRemoteClient[] Clients { get; }
        DateTime LastClientAccess { get; set; }
        bool IsExpired { get; }
        bool IsEmpty { get; }

        void Add(IRemoteClient client);
        void Remove(IRemoteClient client);
        void SendAll(IRoomCommand command, Func<IRemoteClient, bool> filter);
    }
}