using System;

namespace Rooms.Protocol.Sockets
{
    public class SocketSendEventArgs : EventArgs
    {
        public SocketSendEventArgs(BaseSocketClient client, byte[] buffer, int length)
        {
            Client = client;
            Buffer = buffer;
            Length = length;
        }

        public BaseSocketClient Client { get; }
        public byte[] Buffer { get; }
        public int Length { get; }
    }
}