using log4net;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{
    public class RemoteClient : RoomClient, IRemoteClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomClient));

        public RemoteClient(IBaseSocketClient socketClient, IPool<byte[]> bytesPool, IPool<IRoomCommand> poolCommandPool, IProtocolParser protocol) 
            : base(socketClient, bytesPool, poolCommandPool, protocol)
        {
        }

        public void AttachToRoom(IRoomChannel roomChannel)
        {
            if (roomChannel != null)
                Log.Info("Attach client " + ClientId + " to RoomId: " + roomChannel.Name);
            else
                Log.Info("Detach client " + ClientId + "to Room " + Room + "...");

            Room = roomChannel?.Name;
            RoomChannel = roomChannel;
        }

        public IRoomChannel RoomChannel { get; private set; }
    }
}