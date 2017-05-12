namespace Rooms.Protocol.Pooling
{
    public interface IPoolChecker
    {
        bool IsUsed { get; }

        void SetUsed();
        void SetFree();
    }
}