using System;
using System.Net.Sockets;

namespace Rooms.Protocol
{
    public interface IBaseSocketClient: IDisposable
    {
        event EventHandler<UnhandledExceptionEventArgs> Disconnect;
        event EventHandler<SocketReceiveEventArgs> AfterReceive;
        event EventHandler<SocketSendEventArgs> AfterSend;
        bool IsConnected { get; }
        void Attach(Socket socket);
        void Detach();
        void SendBytes(byte[] data, int length);
    }
}