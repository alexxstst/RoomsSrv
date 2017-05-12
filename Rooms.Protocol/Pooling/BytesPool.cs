namespace Rooms.Protocol.Pooling
{
    public class BytesPool: StandartPool<byte[]>
    {
        public BytesPool() : base(() => new byte[2048])
        {
        }
    }
}