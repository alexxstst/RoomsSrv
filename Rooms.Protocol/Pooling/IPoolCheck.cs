namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Интерфейс для проверки того, что объект не используется другими
    /// </summary>
    public interface IPoolChecker
    {
        /// <summary>
        /// Флаг, показывающий, что объект используется.
        /// </summary>
        bool IsUsed { get; }

        /// <summary>
        /// Метод, вызывается при выдачи объекта из пула
        /// </summary>
        void SetUsed();

        /// <summary>
        /// Метод вызывается при добавление объекта в пул
        /// </summary>
        void SetFree();
    }
}