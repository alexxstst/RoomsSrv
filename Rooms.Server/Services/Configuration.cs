using System;
using System.Configuration;
using System.Net;

namespace Rooms.Server.Services
{
    class Configuration : IConfiguration
    {
        public IPEndPoint ListenAdress { get; }
        public int PacketSize { get; }

        public Configuration()
        {
            PacketSize = Convert.ToInt32(ConfigurationManager.AppSettings["bufferSize"]);

            var pairValues = ConfigurationManager.AppSettings["listen"].Split(':');
            ListenAdress = new IPEndPoint(IPAddress.Parse(pairValues[0]), Convert.ToInt32(pairValues[1]));
        }
    }
}