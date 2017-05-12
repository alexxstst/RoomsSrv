using System;
using System.Collections.Concurrent;

namespace Rooms.Protocol.Pooling
{
    public abstract class BasePool<T> 
        : IPool<T> where T:class 
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        protected abstract T CreateItem();
        

        public virtual T Get()
        {
            T result;
            if (!_queue.TryDequeue(out result))
                result = CreateItem();

            var checker = result as IPoolChecker;
            checker?.SetUsed();

            return result;
        }

        public virtual void Free(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var checker = item as IPoolChecker;
            checker?.SetFree();

            _queue.Enqueue(item);
        }

        public int Length => _queue.Count;
    }
}