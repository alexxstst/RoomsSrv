using System;

namespace Rooms.Protocol.Pooling
{
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
                Free(CreateItem());
        }

        protected override T CreateItem()
        {
            return _factoryCallback();
        }
    }
}