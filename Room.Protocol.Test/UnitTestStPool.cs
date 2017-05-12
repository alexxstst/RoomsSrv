using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rooms.Protocol.Pooling;

namespace Room.Protocol.Test
{
    [TestClass]
    public class UnitTestStPool
    {
        [TestMethod]
        public void TestMethodCreated()
        {
            var pool = new StandartPool<object>(() => new object(), 5);

            Assert.AreEqual(5, pool.Length);

            var value = pool.Get();
            Assert.AreEqual(4, pool.Length);

            pool.Free(value);
            Assert.AreEqual(5, pool.Length);
        }

        [TestMethod]
        public void TestMethodCreated2()
        {
            var pool = new StandartPool<object>(() => new object(), 0);

            Assert.AreEqual(0, pool.Length);

            var value = pool.Get();
            Assert.AreEqual(0, pool.Length);

            pool.Free(value);
            Assert.AreEqual(1, pool.Length);
        }

    }
}
