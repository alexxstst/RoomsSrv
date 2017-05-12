using Autofac;
using log4net;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;
using Rooms.Protocol.Sockets;
using Rooms.Server.GameObjects;
using Rooms.Server.Services;
using Rooms.Server.Services.Pooling;

namespace Rooms.Server
{
    class Program : IMainApp
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            _logger.Info("Start simple rooms server.");

            var builder = new ContainerBuilder();
            builder.RegisterType<BytesPool>().As<IPool<byte[]>>().SingleInstance();
            builder.RegisterType<CommandsPool>().As<IPool<IRoomCommand>>().SingleInstance();
            builder.RegisterType<ClientsPool>().As<IPool<IRemoteClient>>().SingleInstance();
            builder.RegisterType<ChannelPool>().As<IPool<IRoomChannel>>().SingleInstance();
            builder.RegisterType<RoomManager>().As<IRoomManager>().SingleInstance();
            builder.RegisterType<RawSocketServer>().As<IRawSocketServer>().SingleInstance();
            builder.RegisterType<Program>().As<IMainApp>().SingleInstance();
            builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();
            builder.RegisterType<SimpleStringProtocolParser>().As<IProtocolParser>().SingleInstance();

            builder.RegisterType<RemoteClient>().As<IRemoteClient>();
            builder.RegisterType<Channel>().As<IRoomChannel>();
            builder.RegisterType<BaseSocketClient>().As<IBaseSocketClient>();

            var container = builder.Build();

            var mainApp = container.Resolve<IMainApp>();
            mainApp.IsRunnnig = true;
            mainApp.Container = container;

            var config = mainApp.Container.Resolve<IConfiguration>();
            _logger.Info("Listen: " + config.ListenAdress);
            _logger.Info("PacketSize: " + config.PacketSize);

            _logger.Info("Wait input connections...");

            var socketServer = mainApp.Container.Resolve<IRawSocketServer>();
            socketServer.Run();

            _logger.Info("Server exited...");
        }

        public bool IsRunnnig { get; set; }
        public IContainer Container { get; set; }
    }

    public interface IMainApp
    {
        bool IsRunnnig { get; set; }
        IContainer Container { get; set; }
    }
}
