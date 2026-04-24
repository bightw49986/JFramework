using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using JFramework.ObjectPooling;

namespace JFramework.Tests.ObjectPooling
{
    [TestFixture]
    public class CSharpObjectPoolTests
    {
        private CSharpObjectPool<TestPoolObject> pool;

        [SetUp]
        public void SetUp()
        {
            pool = new CSharpObjectPool<TestPoolObject>(typeof(TestPoolObject), 3);
        }

        [TearDown]
        public void TearDown()
        {
            pool.Dispose();
        }

        // --- 初始化 ---

        [Test]
        public void Initialization_WithInitialSize5_HasCorrectCounts()
        {
            using var p = new CSharpObjectPool<TestPoolObject>(typeof(TestPoolObject), 5);
            Assert.AreEqual(5, p.TotalCount);
            Assert.AreEqual(5, p.AvailableCount);
        }

        [Test]
        public void Initialization_WithInitialSize0_HasZeroCounts()
        {
            using var p = new CSharpObjectPool<TestPoolObject>(typeof(TestPoolObject), 0);
            Assert.AreEqual(0, p.TotalCount);
            Assert.AreEqual(0, p.AvailableCount);
        }

        // --- Spawn ---

        [Test]
        public void Spawn_ReturnsNonNullObject()
        {
            var obj = pool.Spawn();
            Assert.IsNotNull(obj);
        }

        [Test]
        public void Spawn_DecreasesAvailableCountByOne()
        {
            int before = pool.AvailableCount;
            pool.Spawn();
            Assert.AreEqual(before - 1, pool.AvailableCount);
        }

        [Test]
        public void Spawn_CallsOnSpawnAndSetsIsActiveTrue()
        {
            var obj = pool.Spawn();
            Assert.IsTrue(obj.SpawnCalled);
            Assert.IsTrue(obj.IsActive);
        }

        [Test]
        public void Spawn_WhenPoolEmpty_AutoCreatesNewObject()
        {
            // Drain the pool
            for (int i = 0; i < 3; i++)
                pool.Spawn();

            int totalBefore = pool.TotalCount;
            int availableBefore = pool.AvailableCount;

            var extra = pool.Spawn();

            Assert.AreEqual(totalBefore + 1, pool.TotalCount);
            Assert.AreEqual(0, pool.AvailableCount);
            Assert.IsNotNull(extra);
        }

        [Test]
        public void Spawn_OnBeforeSpawnCallback_IsCalledBeforeOnSpawn()
        {
            bool isActiveInCallback = true;

            pool.Spawn(obj =>
            {
                isActiveInCallback = obj.IsActive;
            });

            Assert.IsFalse(isActiveInCallback);
        }

        // --- Recycle ---

        [Test]
        public void Recycle_IncreasesAvailableCountByOne()
        {
            var obj = pool.Spawn();
            int before = pool.AvailableCount;
            pool.Recycle(obj);
            Assert.AreEqual(before + 1, pool.AvailableCount);
        }

        [Test]
        public void Recycle_CallsOnRecycleAndSetsIsActiveFalse()
        {
            var obj = pool.Spawn();
            pool.Recycle(obj);
            Assert.IsTrue(obj.RecycleCalled);
            Assert.IsFalse(obj.IsActive);
        }

        [Test]
        public void Recycle_NullObject_LogsError()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            pool.Recycle(null);
        }

        [Test]
        public void Recycle_ObjectNotBelongingToPool_LogsError()
        {
            var foreign = new TestPoolObject();
            foreign.OnSpawn(); // IsActive = true，排除 LogWarning 路徑

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            pool.Recycle(foreign);
        }

        [Test]
        public void Recycle_InactiveObject_LogsWarning()
        {
            // Spawn 後立刻 Recycle 讓物件回到非 active 狀態，再嘗試再次 Recycle
            var obj = pool.Spawn();
            pool.Recycle(obj); // 正常回收，IsActive=false

            LogAssert.Expect(LogType.Warning, new Regex(".*"));
            pool.Recycle(obj); // 再次回收不活躍物件 → LogWarning
        }

        // --- 其他 ---

        [Test]
        public void Expand_IncreasesTotalCountByGivenAmount()
        {
            int before = pool.TotalCount;
            pool.Expand(3);
            Assert.AreEqual(before + 3, pool.TotalCount);
        }

        [Test]
        public void Clear_SetsBothCountsToZero()
        {
            pool.Spawn();
            pool.Clear();
            Assert.AreEqual(0, pool.TotalCount);
            Assert.AreEqual(0, pool.AvailableCount);
        }

        [Test]
        public void RecycleRequested_Event_TriggersPoolRecycle()
        {
            var obj = pool.Spawn();
            int availableBefore = pool.AvailableCount;

            obj.TriggerRecycleRequested();

            Assert.AreEqual(availableBefore + 1, pool.AvailableCount);
            Assert.IsFalse(obj.IsActive);
        }

        [Test]
        public void Dispose_CallsDisposeOnAllObjects()
        {
            // Spawn 幾個，讓池內有 active 與 inactive 物件
            var spawned = pool.Spawn();
            // pool 共 3 個物件，其中 1 個被 spawn 出去

            pool.Dispose();

            // 已 spawn 的物件也應被 Dispose
            Assert.IsTrue(spawned.DisposeCalled);
        }
    }
}
