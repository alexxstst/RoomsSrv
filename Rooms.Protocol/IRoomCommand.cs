using System.Collections.Generic;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{
    public interface IRoomCommand : IPoolChecker
    {
        string Command { get; set; }
        Dictionary<string,string> Data { get; }
    }
}