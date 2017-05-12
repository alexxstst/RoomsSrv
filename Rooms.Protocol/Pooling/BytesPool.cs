namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Пул массивов
    /// </summary>
    public class BytesPool: StandartPool<byte[]>
    {
        public BytesPool() : base(() => new byte[2048])
        {
        }
    }
}