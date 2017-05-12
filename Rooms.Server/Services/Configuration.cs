using System;
using System.Configuration;
using System.Net;

namespace Rooms.Server.Services
{
    class Configuration : IConfiguration
    {
        /// <summary>
        /// Адрес для прослушки
        /// </summary>
        public IPEndPoint ListenAdress { get; }

        /// <summary>
        /// Размер пакета в байтах
        /// </summary>
        public int PacketSize { get; }

        public Configuration()
        {
            PacketSize = Convert.ToInt32(ConfigurationManager.AppSettings["bufferSize"]);

            var pairValues = ConfigurationManager.AppSettings["listen"].Split(':');
            ListenAdress = new IPEndPoint(IPAddress.Parse(pairValues[0]), Convert.ToInt32(pairValues[1]));
        }
    }
}