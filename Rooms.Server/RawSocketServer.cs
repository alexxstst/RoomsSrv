using System;
using System.Net.Sockets;
using Rooms.Protocol.Pooling;
using Rooms.Server.GameObjects;
using Rooms.Server.Services;

namespace Rooms.Server
{
    public class RawSocketServer : IRawSocketServer
    {
        private readonly IMainApp _mainApp;
        private readonly IConfiguration _configuration;
        private readonly IPool<IRemoteClient> _pool;
        private readonly IRoomManager _roomManager;
        private readonly TcpListener _tcpListener;

        public RawSocketServer(IMainApp mainApp
            , IConfiguration configuration
            , IPool<IRemoteClient> pool
            , IRoomManager roomManager)
        {
            if (mainApp == null)
                throw new ArgumentNullException(nameof(mainApp));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            if (roomManager == null)
                throw new ArgumentNullException(nameof(roomManager));

            _mainApp = mainApp;
            _configuration = configuration;
            _pool = pool;
            _roomManager = roomManager;
            _tcpListener = new TcpListener(_configuration.ListenAdress);
        }

        public void Run()
        {
            _tcpListener.Start(100);
            while (_mainApp.IsRunnnig)
            {
                var remotClient = _pool.Get();
                remotClient.Attach(_tcpListener.AcceptSocket());
                _roomManager.AttachClient(remotClient);
            }
        }
    }
}