using System;
using Autofac;
using Rooms.Protocol;
using Rooms.Protocol.Pooling;
using Rooms.Server.GameObjects;

namespace Rooms.Server.Services.Pooling
{
    public class ClientsPool: BasePool<IRemoteClient>
    {
        private readonly IMainApp _mainApp;

        public ClientsPool(IMainApp mainApp)
        {
            if (mainApp == null)
                throw new ArgumentNullException(nameof(mainApp));

            _mainApp = mainApp;
        }

        protected override IRemoteClient CreateItem()
        {
            return _mainApp.Container.Resolve<IRemoteClient>();
        }

    }
}