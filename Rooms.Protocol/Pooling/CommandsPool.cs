using System;

namespace Rooms.Protocol.Pooling
{
    public class CommandsPool : StandartPool<IRoomCommand>
    {
        public CommandsPool() : base(() =>
            {
                var cmd = new RoomCommand();
                cmd.SetUsed();
                return cmd;
            }, 1000)

        {
        }

        public override void Free(IRoomCommand item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            item.Data.Clear();
            base.Free(item);
        }
    }
}