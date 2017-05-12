using System;
using Rooms.Protocol.Sockets;

namespace Rooms.Protocol
{
    public class SocketReceiveEventArgs: EventArgs
    {
        public SocketReceiveEventArgs(BaseSocketClient client, byte[] buffer, int length)
        {
            Client = client;
            Buffer = buffer;
            Length = length;
        }

        public BaseSocketClient Client { get; }
        public byte[] Buffer { get; }
        public int Length { get; }
        public int ReadedLength { get; set; }
    }
}