using Rooms.Protocol;

namespace Rooms.Server.GameObjects
{
    public interface IRemoteClient : IRoomClient
    {
        void AttachToRoom(IRoomChannel roomChannel);

        IRoomChannel RoomChannel { get; }
    }
}