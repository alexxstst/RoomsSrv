using System;
using Autofac;
using Rooms.Protocol.Pooling;
using Rooms.Server.GameObjects;

namespace Rooms.Server.Services.Pooling
{

    public class ChannelPool : BasePool<IRoomChannel>
    {
        private readonly IMainApp _mainApp;

        public ChannelPool(IMainApp mainApp)
        {
            if (mainApp == null)
                throw new ArgumentNullException(nameof(mainApp));

            _mainApp = mainApp;
        }

        protected override IRoomChannel CreateItem()
        {
            return _mainApp.Container.Resolve<IRoomChannel>();
        }

    }

}