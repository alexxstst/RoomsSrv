using System;

namespace Rooms.Protocol.Pooling
{

    /// <summary>
    /// Базовый класс поддерживающий интерфейс для проверки объекта в использовании
    /// </summary>
    public class BasePoolChecker : IPoolChecker
    {
        private bool _isUsed;

        /// <summary>
        /// Флаг, показывающий, что объект используется.
        /// </summary>
        public bool IsUsed => _isUsed;

        /// <summary>
        /// Метод, вызывается при выдачи объекта из пула
        /// </summary>
        public void SetUsed()
        {
            lock (this)
            {
                if (IsUsed)
                    throw new InvalidOperationException("Object already used!");

                _isUsed = true;
            }
        }

        /// <summary>
        /// Метод вызывается при добавление объекта в пул
        /// </summary>
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