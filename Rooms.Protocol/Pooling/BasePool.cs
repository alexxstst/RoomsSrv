using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Rooms.Protocol.Pooling
{
    public abstract class BasePool<T> 
        : IPoolStatistics, IPool<T> where T:class 
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        protected long _getCallCounter;
        protected long _freeCounter;
        protected long _usedObjects;
        protected long _pooledObjects;
        protected long _createdObjects;

        /// <summary>
        /// Перегружаемый метод в наследуемых классах
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateItem();


        /// <summary>
        /// Метод для получения объекта из пула
        /// </summary>
        /// <returns></returns>
        public virtual T Get()
        {
            T result;
            if (!_queue.TryDequeue(out result))
            {
                result = CreateItem();
                Interlocked.Increment(ref _createdObjects);
            }
            else
            {
                Interlocked.Decrement(ref _pooledObjects);
            }

            (result as IPoolChecker)?.SetUsed();

            Interlocked.Increment(ref _getCallCounter);
            Interlocked.Increment(ref _usedObjects);
            return result;
        }

        /// <summary>
        /// Метод для возврата объекта в пул
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Free(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            (item as IPoolChecker)?.SetFree();

            Interlocked.Increment(ref _freeCounter);
            Interlocked.Increment(ref _pooledObjects);
            Interlocked.Decrement(ref _usedObjects);
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Свойство показывающее количество объектов в пуле
        /// </summary>
        public int Length => _queue.Count;

        public IPoolStatistics Statistics => this;

        long IPoolStatistics.GetCallCounter => _getCallCounter;
        long IPoolStatistics.FreeCounter => _freeCounter;
        long IPoolStatistics.UsedObjects => _usedObjects;
        long IPoolStatistics.PooledObjects => _pooledObjects;
        long IPoolStatistics.CreatedObjects => _createdObjects;
    }
}