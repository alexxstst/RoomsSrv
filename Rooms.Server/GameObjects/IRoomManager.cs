namespace Rooms.Server.GameObjects
{
    public interface IRoomManager
    {
        IRoomChannel[] Rooms { get; }

        IRoomChannel GetRoom(string roomId);
        void FreeRoom(IRoomChannel roomChannel, RoomRemoveReason reason);
        void AttachClient(IRemoteClient remotClient);
    }

    public enum RoomRemoveReason
    {
        Timeout,
        DisconnectAllClients,
        ServerStop
    }
}