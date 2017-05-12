using System.Net;

namespace Rooms.Server.Services
{

    /// <summary>
    /// Интерфейс конфигурации
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Адрес для прослушки
        /// </summary>
        IPEndPoint ListenAdress { get; }

        /// <summary>
        /// Размер пакета в байтах
        /// </summary>
        int PacketSize { get; }
    }
}