using System;
using Autofac;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;

namespace Rooms.Client.Pooling
{

    public class ClientsPool : BasePool<IRoomClient>
    {
        protected override IRoomClient CreateItem()
        {
            return FormMain.Container.Resolve<IRoomClient>();
        }

    }
}