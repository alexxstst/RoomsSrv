using System.Collections.Generic;
using System.Text;
using Rooms.Protocol.Pooling;

namespace Rooms.Protocol
{
    public class RoomCommand : BasePoolChecker, IRoomCommand
    {
        public string Command { get; set; }
        public Dictionary<string,string> Data { get; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var sb = new StringBuilder(2048);

            sb.Append(Command);
            sb.Append("|");

            foreach (var pair in Data)
            {
                sb.Append(pair.Key);
                sb.Append("^");
                sb.Append(pair.Value);
            }

            return sb.ToString();
        }
    }
}