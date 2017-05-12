using System;

namespace Rooms.Protocol.Pooling
{
    public class BasePoolChecker : IPoolChecker
    {
        private bool _isUsed;

        public bool IsUsed => _isUsed;

        public void SetUsed()
        {
            lock (this)
            {
                if (IsUsed)
                    throw new InvalidOperationException("Object already used!");

                _isUsed = true;
            }
        }

        public void SetFree()
        {
            lock (this)
            {
                if (!IsUsed)
                    throw new InvalidOperationException("Object already free!");

                _isUsed = false;
            }
        }
    }
}