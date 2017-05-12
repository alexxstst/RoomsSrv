using Autofac;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Room.ConsoleClient.Pooling
{

    public class ClientsPool : BasePool<IRoomClient>
    {
        protected override IRoomClient CreateItem()
        {
            return Program.Container.Resolve<IRoomClient>();
        }

    }
}