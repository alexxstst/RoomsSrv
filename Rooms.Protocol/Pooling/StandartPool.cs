using System;
using System.Threading;

namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Пулиенг с возможностью использования внешней функции для создания объектов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StandartPool<T> 
        : BasePool<T> where T:class 
    {
        private readonly Func<T> _factoryCallback;

        public StandartPool(Func<T> func, int initCount = 10)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (initCount < 0)
                throw new ArgumentException("initCount less 0");

            _factoryCallback = func;
            while (initCount-- > 0)
            {
                Interlocked.Increment(ref _createdObjects);
                Interlocked.Increment(ref _usedObjects);
                Free(CreateItem());
            }
        }

        protected override T CreateItem()
        {
            return _factoryCallback();
        }
    }
}