using log4net;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;
using Rooms.Protocol.Sockets;

namespace Rooms.Server.GameObjects
{
    public class RemoteClient : RoomClient, IRemoteClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomClient));

        public RemoteClient(IBaseSocketClient socketClient, IPool<byte[]> bytesPool, IPool<IRoomCommand> poolCommandPool, IProtocolParser protocol) 
            : base(socketClient, bytesPool, poolCommandPool, protocol)
        {
        }

        /// <summary>
        /// Метод, подключающий клиента к комнате
        /// </summary>
        /// <param name="roomChannel"></param>
        public void AttachToRoom(IRoomChannel roomChannel)
        {
            if (roomChannel != null)
                Log.Info("Attach client " + ClientId + " to RoomId: " + roomChannel.RoomId);
            else
                Log.Info("Detach client " + ClientId + "to Room " + Room + "...");

            Room = roomChannel?.RoomId;
            RoomChannel = roomChannel;
        }

        /// <summary>
        /// Комната, в которой находиться клиент
        /// </summary>
        public IRoomChannel RoomChannel { get; private set; }
    }
}