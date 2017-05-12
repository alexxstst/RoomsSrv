using System;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol.Parser
{
    public interface IProtocolParser
    {
        IRoomCommand FromBuffer(byte[] buffer, int startIndex, int length, out int resultLength);
        Tuple<byte[], int> ToBuffer(IRoomCommand command);
    }
}