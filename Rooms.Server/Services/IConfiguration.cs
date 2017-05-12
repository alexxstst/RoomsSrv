using System.Net;

namespace Rooms.Server.Services
{
    public interface IConfiguration
    {
        IPEndPoint ListenAdress { get; }
        int PacketSize { get; }
    }
}