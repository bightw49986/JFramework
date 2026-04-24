using System;
using JFramework.ObjectPooling;

namespace JFramework.Tests.ObjectPooling
{
    public class TestPoolObject : IPoolableObject<Type>, IDisposable
    {
        public event Action<IPoolableObject> RecycleRequested;

        public bool IsActive { get; private set; }
        public string Name { get; set; }
        public Type Prototype { get; set; }

        public bool SpawnCalled { get; private set; }
        public bool RecycleCalled { get; private set; }
        public bool DisposeCalled { get; private set; }

        public void OnSpawn()
        {
            IsActive = true;
            SpawnCalled = true;
        }

        public void OnRecycle()
        {
            IsActive = false;
            RecycleCalled = true;
        }

        public void Dispose()
        {
            DisposeCalled = true;
        }

        public void ResetTracking()
        {
            SpawnCalled = false;
            RecycleCalled = false;
            DisposeCalled = false;
        }

        public void TriggerRecycleRequested()
        {
            RecycleRequested?.Invoke(this);
        }
    }
}
