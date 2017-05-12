namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Базовый интерфейс пулинга
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// Метод для получения объекта из пула
        /// </summary>
        /// <returns></returns>
        T Get();

        /// <summary>
        /// Метод для возврата объекта в пул
        /// </summary>
        /// <param name="buffer"></param>
        void Free(T buffer);

        /// <summary>
        /// Свойство показывающее количество объектов в пуле
        /// </summary>
        int Length { get; }

        IPoolStatistics Statistics { get; }
    }
}