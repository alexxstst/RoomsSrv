namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Интерфейс статистики по использованию пула
    /// </summary>
    public interface IPoolStatistics
    {
        /// <summary>
        /// Количество вызовов метода для получения объекта
        /// </summary>
        long GetCallCounter { get; }

        /// <summary>
        /// Количество вызовов метода на возврат объекта
        /// </summary>
        long FreeCounter { get; }

        /// <summary>
        /// Количество переданных на использование объектов
        /// </summary>
        long UsedObjects { get; }

        /// <summary>
        /// Количество объектов хранящихся в пуле
        /// </summary>
        long PooledObjects { get; }

        /// <summary>
        /// Количество объектов, которые были созданы пулом дополнительно, к уже имеющимся
        /// </summary>
        long CreatedObjects { get; }
    }
}