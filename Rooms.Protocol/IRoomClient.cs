using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{
    public interface IRoomClient: IPoolChecker, IDisposable
    {
        string Room { get; set; }
        string ClientId { get; set; }

        void SendCommand(IRoomCommand command);
        void Detach();
        void Attach(Socket socket);

        bool IsConnected { get; }

        event Action<IRoomClient, IRoomCommand> AfterReceive;
        event EventHandler<UnhandledExceptionEventArgs> Disconnect;
    }


}