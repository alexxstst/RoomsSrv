using System;
using System.Text;

namespace Rooms.Protocol.Pooling
{
    public class StringPool : StandartPool<StringBuilder>
    {
        public StringPool()
            : base(() => new StringBuilder(), 0)
        {
        }
    }
}