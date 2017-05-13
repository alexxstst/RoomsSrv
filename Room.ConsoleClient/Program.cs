using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Autofac;
using Room.ConsoleClient.Pooling;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;
using Rooms.Protocol.Sockets;

namespace Room.ConsoleClient
{
    class Program
    {
        private static IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2020);
        private static string _roomId = Guid.NewGuid().ToString();
        private static bool _stopRun;
        internal static readonly IContainer Container;


        static Program()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<BytesPool>().As<IPool<byte[]>>().SingleInstance();
            builder.RegisterType<CommandsPool>().As<IPool<IRoomCommand>>().SingleInstance();
            builder.RegisterType<ClientsPool>().As<IPool<IRoomClient>>().SingleInstance();
            builder.RegisterType<StringPool>().As<IPool<StringBuilder>>().SingleInstance();
            builder.RegisterType<SimpleStringProtocolParser>().As<IProtocolParser>().SingleInstance();

            builder.RegisterType<RoomClient>().As<IRoomClient>();
            builder.RegisterType<BaseSocketClient>().As<IBaseSocketClient>();

            Container = builder.Build();
        }

        static void Main(string[] args)
        {
            ProcessCommandArgs(args);

            var socketPool = Container.Resolve<IPool<IRoomClient>>();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(_endPoint);

            var roomClient = socketPool.Get();
            roomClient.Attach(socket);
            roomClient.Room = _roomId;

            roomClient.AfterReceive += OnClientReceive;
            roomClient.Disconnect += OnDisconnect;

            var _msgSendThread = new Thread(OnSendMessage);
            _msgSendThread.Start(roomClient);

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            _stopRun = true;
            _msgSendThread.Join();
        }

        private static void OnSendMessage(object obj)
        {
            var client = (IRoomClient) obj;
            while (!_stopRun)
            {
                Thread.Sleep(100);
                    SendClientCommand(client, cmd =>
                    {
                        cmd.Command = Commands.PushMessage;
                        cmd.Data["RandomString"] = GenerateRandomString(900);
                    });
            }
        }

        private static void OnDisconnect(object sender, UnhandledExceptionEventArgs e)
        {
            var roomClient = (IRoomClient) sender;
            roomClient.AfterReceive += OnClientReceive;
            roomClient.Disconnect += OnDisconnect;
        }

        private static void OnClientReceive(IRoomClient client, IRoomCommand command)
        {
            switch (command.Command)
            {
                case Commands.SetClientId:
                    client.ClientId = command.Data["ClientId"];
                    SendClientCommand(client, cmd =>
                    {
                        cmd.Command = Commands.EnterToRoom;
                        cmd.Data["RoomId"] = client.Room;
                    });
                    break;

                case Commands.PushMessage:
                    break;
            }
        }

        private static readonly Random random = new Random();
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void SendClientCommand(IRoomClient client, Action<IRoomCommand> action)
        {
            IRoomCommand command = null;
            var pool = Container.Resolve<IPool<IRoomCommand>>();

            try
            {
                command = pool.Get();
                action(command);

                client.SendCommand(command);
            }
            catch (Exception e)
            {
                pool.Free(command);
            }
            finally
            {
                if (command != null)
                    pool.Free(command);
            }
        }

        private static void ProcessCommandArgs(string[] args)
        {
            if (args.Length > 2)
            {
                var svalues = args[1].Split(':');
                _endPoint = new IPEndPoint(IPAddress.Parse(svalues[0]), Convert.ToInt32(svalues[1]));
            }

            if (args.Length > 2)
            {
                _roomId = args[2];
            }
        }
    }
}
