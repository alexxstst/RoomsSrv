namespace Rooms.Server
{

    /// <summary>
    /// Интерфейс для прослушки сокета
    /// </summary>
    public interface IRawSocketServer
    {
        /// <summary>
        /// Метод, принимающий подключения от клиентов
        /// </summary>
        void Run();
    }
}