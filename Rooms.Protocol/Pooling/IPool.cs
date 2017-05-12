namespace Rooms.Protocol.Pooling
{
    public interface IPool<T>
    {
        T Get();
        void Free(T buffer);
        int Length { get; }
    }
}