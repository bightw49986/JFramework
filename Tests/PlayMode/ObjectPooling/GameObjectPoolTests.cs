using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using JFramework.ObjectPooling;

namespace JFramework.Tests.PlayMode.ObjectPooling
{
    [TestFixture]
    public class GameObjectPoolTests
    {
        private GameObject prefabGo;
        private Poolable prefab;
        private GameObject containerGo;
        private GameObjectPool pool;

        [SetUp]
        public void SetUp()
        {
            // 建立假 prefab：一般遊戲中是 Asset，測試裡直接用 new GameObject 代替
            prefabGo = new GameObject("TestPrefab");
            prefab = prefabGo.AddComponent<Poolable>();

            containerGo = new GameObject("PoolContainer");
            pool = new GameObjectPool(prefab, containerGo.transform, initialSize: 3);
        }

        [TearDown]
        public void TearDown()
        {
            pool.Dispose();
            Object.DestroyImmediate(containerGo);
            Object.DestroyImmediate(prefabGo);
        }

        // ─── 這個測試用 [Test] 就夠了，不需要等幀 ────────────────────────────

        [Test]
        public void Initialization_SpawnedObjectsAreInactive()
        {
            // 剛建立的 pool，所有物件應該是 inactive（等待被 Spawn）
            Assert.AreEqual(3, pool.TotalCount);
            Assert.AreEqual(3, pool.AvailableCount);
        }

        // ─── 以下用 [UnityTest]，因為需要等 Awake 執行 ────────────────────────

        [UnityTest]
        public IEnumerator Spawn_ActivatesGameObject()
        {
            // yield return null = 等一幀，讓剛建立的 GameObject 跑完 Awake/Start
            yield return null;

            var obj = pool.Spawn();

            Assert.IsTrue(obj.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Spawn_SetsIsActiveTrue()
        {
            yield return null;

            var obj = pool.Spawn();

            Assert.IsTrue(obj.IsActive);
        }

        [UnityTest]
        public IEnumerator Recycle_DeactivatesGameObject()
        {
            yield return null;

            var obj = pool.Spawn();
            pool.Recycle(obj);

            Assert.IsFalse(obj.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Recycle_SetsIsActiveFalse()
        {
            yield return null;

            var obj = pool.Spawn();
            pool.Recycle(obj);

            Assert.IsFalse(obj.IsActive);
        }

        [UnityTest]
        public IEnumerator Spawn_ResetsTransformToInitialState()
        {
            yield return null;

            // 先 spawn，移動到別的位置，再回收，再重新 spawn
            var obj = pool.Spawn();
            obj.transform.position = new Vector3(99f, 99f, 99f);
            pool.Recycle(obj);

            var respawned = pool.Spawn();

            // resetTransformOnSpawn 預設為 true，應該回到原點
            Assert.That(respawned.transform.position.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(respawned.transform.position.y, Is.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator RecycleSelf_ReturnsObjectToPool()
        {
            yield return null;

            var obj = pool.Spawn();
            int availableBefore = pool.AvailableCount;

            obj.RecycleSelf();

            Assert.AreEqual(availableBefore + 1, pool.AvailableCount);
            Assert.IsFalse(obj.IsActive);
        }

        [UnityTest]
        public IEnumerator SpawnedObject_ParentIsSetToContainer_AfterRecycle()
        {
            yield return null;

            var obj = pool.Spawn();
            // Spawn 後 detachParentOnTransformReset=true，parent 應為 null
            Assert.IsNull(obj.transform.parent);

            pool.Recycle(obj);
            // 回收後應歸還到 poolContainer
            Assert.AreEqual(containerGo.transform, obj.transform.parent);
        }
    }
}
