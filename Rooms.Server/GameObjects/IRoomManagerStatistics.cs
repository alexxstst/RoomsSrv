namespace Rooms.Server.GameObjects
{
    public interface IRoomManagerStatistics
    {
        long ClientConnected { get; }
        long ClinetDisconnected { get; }
        long ActiveRoomsCount { get; }
        long ReceiveCommandsCount { get; }
        long SendCommandsCount { get; }
        long CurrentTrottleReceive { get; }
        long CurrentTrottleSend { get; }
    }
}