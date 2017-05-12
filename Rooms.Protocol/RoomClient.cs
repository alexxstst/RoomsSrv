using System;
using System.Net.Sockets;
using log4net;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{
    public class RoomClient : BasePoolChecker, IRoomClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomClient));

        private readonly IBaseSocketClient _baseSocketClient;
        private readonly IPool<byte[]> _pool;
        private readonly IPool<IRoomCommand> _poolCommand;
        private readonly IProtocolParser _protocol;

        public RoomClient(IBaseSocketClient socketClient, IPool<byte[]> bytesPool, IPool<IRoomCommand> poolCommandPool,
            IProtocolParser protocol)
        {
            if (bytesPool == null)
                throw new ArgumentNullException(nameof(bytesPool));

            if (poolCommandPool == null)
                throw new ArgumentNullException(nameof(poolCommandPool));

            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            if (socketClient == null)
                throw new ArgumentNullException(nameof(socketClient));

            _pool = bytesPool;
            _protocol = protocol;
            _poolCommand = poolCommandPool;

            _baseSocketClient = socketClient;
            _baseSocketClient.Disconnect += OnDisconnect;
            _baseSocketClient.AfterSend += (s, e) => _pool.Free(e.Buffer);
            _baseSocketClient.AfterReceive += OnAfterReceive;

        }

        private void OnDisconnect(object sender, UnhandledExceptionEventArgs e)
        {
            Disconnect?.Invoke(this, e);
            Detach();
        }

        private void OnAfterReceive(object sender, SocketReceiveEventArgs e)
        {
            var lastIndex = 0;
            var command = _protocol.FromBuffer(e.Buffer, lastIndex, e.Length, out lastIndex);
            while (command != null)
            {
                DoCommandProcess(command);
                command = _protocol.FromBuffer(e.Buffer, lastIndex, e.Length, out lastIndex);
            }

            e.ReadedLength = lastIndex;
        }

        private void DoCommandProcess(IRoomCommand command)
        {
            try
            {
                AfterReceive?.Invoke(this, command);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            finally
            {
                _poolCommand.Free(command);
            }
        }


        private void LogException(Exception e)
        {
            Log.Error(e.ToString());
        }


        public void Dispose()
        {
            _baseSocketClient.Dispose();
        }

        public string Room { get; set; }
        public string ClientId { get; set; }

        public void SendCommand(IRoomCommand command)
        {
            var data = _protocol.ToBuffer(command);
            _baseSocketClient.SendBytes(data.Item1, data.Item2);
        }

        public void Detach()
        {
            _baseSocketClient.Detach();
            Room = null;
            ClientId = null;
        }

        public void Attach(Socket socket)
        {
            _baseSocketClient.Attach(socket);
        }

        public bool IsConnected => _baseSocketClient.IsConnected;

        public event Action<IRoomClient, IRoomCommand> AfterReceive;
        public event EventHandler<UnhandledExceptionEventArgs> Disconnect;
    }
}