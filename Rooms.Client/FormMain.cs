using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using Rooms.Client.Pooling;
using Rooms.Protocol;
using Rooms.Protocol.Parser;
using Rooms.Protocol.Pooling;
using Rooms.Protocol.Sockets;

namespace Rooms.Client
{
    public partial class FormMain : Form
    {
        internal static readonly IContainer Container;
        private readonly List<IRoomClient> _clients = new List<IRoomClient>();
        private Thread _msgSendThread;

        static FormMain()
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

        public FormMain()
        {
            InitializeComponent();

        }

        private void OnStartTestClick(object sender, EventArgs e)
        {
            var groupCount = Convert.ToInt32(textBoxGroups.Text);

            var ipStrValues = textBoxIP.Text.Split(':');
            var ipEdPoint = new IPEndPoint(IPAddress.Parse(ipStrValues[0]), Convert.ToInt32(ipStrValues[1]));

            ThreadPool.QueueUserWorkItem(state =>
            {
                var socketPool = Container.Resolve<IPool<IRoomClient>>();
                var random = new Random();
                for (var i = 0; i < groupCount; i++)
                {
                    var socketCount = random.Next(3, 25);
                    var roomId = Guid.NewGuid().ToString();
                    for (var j = 0; j < socketCount; j++)
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(ipEdPoint);

                        var roomClient = socketPool.Get();
                        roomClient.Attach(socket);
                        roomClient.Room = roomId;

                        OnClientConnected(roomClient);
                    }
                }

                _msgSendThread = new Thread(OnSendMessage);
                _msgSendThread.Start();

            });
        }

        private void OnSendMessage()
        {
            while (_clients.Count > 0)
            {
                Thread.Sleep(100);
                foreach (var client in _clients.ToArray())
                    SendClientCommand(client, cmd =>
                    {
                        cmd.Command = Commands.PushMessage;
                        cmd.Data["RandomString"] = GenerateRandomString(900);
                    });
            }
        }

        private static readonly Random random = new Random();
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void OnClientConnected(IRoomClient roomClient)
        {
            roomClient.AfterReceive += OnClientReceive;
            roomClient.Disconnect += OnDisconnect;

            lock (_clients)
                _clients.Add(roomClient);
        }

        private void OnDisconnect(object sender, UnhandledExceptionEventArgs e)
        {
            lock (_clients)
                _clients.Remove((IRoomClient)sender);
        }

        private void OnClientReceive(IRoomClient client, IRoomCommand command)
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

        private void OnStopTestClick(object sender, EventArgs e)
        {
            var socketPool = Container.Resolve<IPool<IRoomClient>>();
            var items = _clients.ToArray();

            foreach (var client in items)
            {
                client.Detach();
                socketPool.Free(client);
            }

            _msgSendThread?.Join();
        }

        private void SendClientCommand(IRoomClient client, Action<IRoomCommand> action)
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

    }
}
