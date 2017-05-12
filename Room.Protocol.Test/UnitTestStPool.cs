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

            Assert.AreEqual(5, pool.Statistics.CreatedObjects);
            Assert.AreEqual(5, pool.Statistics.FreeCounter);
            Assert.AreEqual(0, pool.Statistics.GetCallCounter);
            Assert.AreEqual(5, pool.Statistics.PooledObjects);
            Assert.AreEqual(0, pool.Statistics.UsedObjects);

            var value = pool.Get();
            Assert.AreEqual(4, pool.Length);

            Assert.AreEqual(5, pool.Statistics.CreatedObjects);
            Assert.AreEqual(5, pool.Statistics.FreeCounter);
            Assert.AreEqual(1, pool.Statistics.GetCallCounter);
            Assert.AreEqual(4, pool.Statistics.PooledObjects);
            Assert.AreEqual(1, pool.Statistics.UsedObjects);

            pool.Free(value);
            Assert.AreEqual(5, pool.Length);
            Assert.AreEqual(5, pool.Statistics.CreatedObjects);
            Assert.AreEqual(6, pool.Statistics.FreeCounter);
            Assert.AreEqual(1, pool.Statistics.GetCallCounter);
            Assert.AreEqual(5, pool.Statistics.PooledObjects);
            Assert.AreEqual(0, pool.Statistics.UsedObjects);
        }

        [TestMethod]
        public void TestMethodCreated2()
        {
            var pool = new StandartPool<object>(() => new object(), 0);

            Assert.AreEqual(0, pool.Length);

            Assert.AreEqual(0, pool.Statistics.CreatedObjects);
            Assert.AreEqual(0, pool.Statistics.FreeCounter);
            Assert.AreEqual(0, pool.Statistics.GetCallCounter);
            Assert.AreEqual(0, pool.Statistics.PooledObjects);
            Assert.AreEqual(0, pool.Statistics.UsedObjects);

            var value = pool.Get();
            Assert.AreEqual(0, pool.Length);

            Assert.AreEqual(1, pool.Statistics.CreatedObjects);
            Assert.AreEqual(0, pool.Statistics.FreeCounter);
            Assert.AreEqual(1, pool.Statistics.GetCallCounter);
            Assert.AreEqual(0, pool.Statistics.PooledObjects);
            Assert.AreEqual(1, pool.Statistics.UsedObjects);

            pool.Free(value);
            Assert.AreEqual(1, pool.Length);

            Assert.AreEqual(1, pool.Statistics.CreatedObjects);
            Assert.AreEqual(1, pool.Statistics.FreeCounter);
            Assert.AreEqual(1, pool.Statistics.GetCallCounter);
            Assert.AreEqual(1, pool.Statistics.PooledObjects);
            Assert.AreEqual(0, pool.Statistics.UsedObjects);

        }

    }
}
