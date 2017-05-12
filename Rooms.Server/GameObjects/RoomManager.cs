using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using log4net;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Server.GameObjects
{

    public class RoomManager : IRoomManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(RoomManager));
        private readonly IMainApp _mainApp;
        private readonly IPool<IRoomCommand> _pool;
        private readonly IPool<IRoomChannel> _poolChannels;
        private readonly ConcurrentDictionary<string, IRemoteClient> _clients = new ConcurrentDictionary<string, IRemoteClient>();
        private readonly Dictionary<string, Action<IRemoteClient, IRoomCommand>> _handlers = new Dictionary<string, Action<IRemoteClient, IRoomCommand>>();
        private readonly Dictionary<string, IRoomChannel> _channels = new Dictionary<string, IRoomChannel>();

        public RoomManager(IMainApp mainApp, IPool<IRoomCommand> pool, IPool<IRoomChannel> poolChannels)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            if (poolChannels == null)
                throw new ArgumentNullException(nameof(poolChannels));

            if (mainApp == null)
                throw new ArgumentNullException(nameof(mainApp));

            _mainApp = mainApp;
            _pool = pool;
            _poolChannels = poolChannels;
            _handlers.Add(Commands.EnterToRoom, OnClientEnterToRoom);
            _handlers.Add(Commands.PushMessage, OnPushMessage);

            var grabageThread = new Thread(OnGarbageRooms);
            grabageThread.Start();
        }

        /// <summary>
        /// Метод, выполняющий очистку комнат с истекшим таймаутом
        /// </summary>
        private void OnGarbageRooms()
        {
            while (_mainApp.IsRunnnig)
            {
                //запускатся каждую секунду
                Thread.Sleep(1000);

                IRoomChannel[] localChannels;
                lock (_channels)
                {
                    //простой фильтр
                    localChannels = _channels
                        .Where(x => x.Value.IsExpired)
                        .Select(x => x.Value)
                        .ToArray();
                }

                //удаляем комнаты
                foreach (var localChannel in localChannels)
                    FreeRoom(localChannel, RoomRemoveReason.Timeout);
            }

        }

        /// <summary>
        /// Метод, отправляющий клиенту команду и утилизирующий её обратно в буфер
        /// TODO перенести метод в IRemoteClient
        /// </summary>
        /// <param name="client"></param>
        /// <param name="action"></param>
        private void SendClientCommand(IRemoteClient client, Action<IRoomCommand> action)
        {
            IRoomCommand command = null;
            try
            {
                command = _pool.Get();
                action(command);

                _log.Debug("SendCommand: " + command);
                client.SendCommand(command);
            }
            catch (Exception e)
            {
                _pool.Free(command);
                _log.Error("Error SendCommand:\n" + e.ToString());
            }
            finally
            {
                if (command != null)
                    _pool.Free(command);
            }
        }

        /// <summary>
        /// Список всех созданных комнат
        /// </summary>
        public IRoomChannel[] Rooms
        {
            get
            {
                lock (_channels)
                    return _channels.Values.ToArray();
            }
        }

        /// <summary>
        /// Метод, возвращает комнату по её идентификатору
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public IRoomChannel GetRoom(string roomId)
        {
            IRoomChannel channel;
            lock (_channels)
                _channels.TryGetValue(roomId, out channel);

            return channel;
        }

        /// <summary>
        /// Метод, удаляет комнату по её идентификатору
        /// </summary>
        /// <param name="roomChannel"></param>
        /// <param name="reason"></param>
        public void FreeRoom(IRoomChannel roomChannel, RoomRemoveReason reason)
        {
            bool successRemove;
            lock (_channels)
                successRemove = _channels.Remove(roomChannel.RoomId);

            //проверяем, что комната уже удалена
            if (successRemove)
            {
                _log.Info("Free roomId: " + roomChannel.RoomId + " reason: " + reason);
                foreach (var roomChannelClient in roomChannel.Clients)
                    roomChannelClient.Detach();

                _poolChannels.Free(roomChannel);
            }
        }

        /// <summary>
        /// Метод, присоеденяет клиента к общему потоку обработки данных
        /// </summary>
        /// <param name="remotClient"></param>
        public void AttachClient(IRemoteClient remotClient)
        {
            //присваеваем новый идентификатор
            remotClient.ClientId = Guid.NewGuid().ToString();

            //подключаемся к событиям
            remotClient.AfterReceive += OnClientCommand;
            remotClient.Disconnect += OnClientDisconnect;

            //добавляем клиента в массив
            _clients.AddOrUpdate(remotClient.ClientId, remotClient, (s, client) => remotClient);

            _log.Info("Add client with id: " + remotClient.ClientId + ". Clients: " + _clients.Count);

            //отправляем клиенту сообщение о присвоении ему ID
            SendClientCommand(remotClient, cmd =>
            {
                cmd.Command = Commands.SetClientId;
                cmd.Data["ClientId"] = remotClient.ClientId;
            });
        }

        private void OnClientCommand(IRoomClient client, IRoomCommand command)
        {
            Action<IRemoteClient, IRoomCommand> handlerAction;
            if (_handlers.TryGetValue(command.Command, out handlerAction))
            {
                //Передача управления обработчику команды
                handlerAction((IRemoteClient) client, command);
            }
            else
                //отключаем клиента, чтобы не слал что попало
                client.Detach();
        }

        private void OnClientDisconnect(object sender, UnhandledExceptionEventArgs e)
        {
            var client = (IRemoteClient) sender;

            //отключаем клиента от событий
            client.AfterReceive -= OnClientCommand;
            client.Disconnect -= OnClientDisconnect;

            if (!string.IsNullOrEmpty(client.ClientId))
            {
                //удаляем клиента из хранилища клиентов
                IRemoteClient result;
                _clients.TryRemove(client.ClientId, out result);
            }

            //удаляем клиента из комнаты
            var roomChannel = client.RoomChannel;
            roomChannel?.Remove(client);

            //логируем
            if (e.ExceptionObject != null)
                _log.Info("Disconnect client " + client.ClientId + " with exception: \n" + e.ExceptionObject);
            else
                _log.Info("Disconnect client " + client.ClientId + " .");

            //отключаем на всякий случай, если клиент уже отключен, то ничего страшного
            client.Detach();
        }

        /// <summary>
        /// Клиент входит в комнату
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command"></param>
        private void OnClientEnterToRoom(IRemoteClient client, IRoomCommand command)
        {
            //пытаемся получить номер комнаты
            string roomId;
            if (!command.Data.TryGetValue("RoomId", out roomId))
            {
                _log.ErrorFormat("Client {0} try create room with empty Id", client.ClientId);
                client.Detach();
                return;
            }

            //устанавливаем номер комнаты
            client.Room = roomId;

            //возвращаем или создаем комнату
            var room = GetRoom(roomId) ?? CreateRoom(roomId);

            //добавляем туда клиента
            room.Add(client);
        }

        /// <summary>
        /// Метод, создающий новую комнату
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        private IRoomChannel CreateRoom(string roomId)
        {
            lock (_channels)
            {
                IRoomChannel roomChannel;
                if (!_channels.TryGetValue(roomId, out roomChannel))
                {
                    roomChannel = _poolChannels.Get();
                    roomChannel.RoomId = roomId;
                    roomChannel.LastClientAccess = DateTime.Now;
                    
                    _channels[roomId] = roomChannel;
                    _log.Info("Create roomId: " + roomId);
                }

                return roomChannel;
            }
        }

        /// <summary>
        /// Метод, при получении сообщения от клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command"></param>
        private void OnPushMessage(IRemoteClient client, IRoomCommand command)
        {
            if (client.RoomChannel == null)
            {
                client.Detach();
                return;
            }

            //если у комнаты ещё не истекло время, то отправляем команду
            if (!client.RoomChannel.IsExpired)
                client.RoomChannel.SendAll(command, c => c != client);
        }

    }

}