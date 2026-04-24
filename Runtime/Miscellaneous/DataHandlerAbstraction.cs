/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework
{
    public interface IDataHandler<T>
    {
        void HandleData(T data);
    }

    public interface IDataProvider<T>
    {
        T GetData();
    }

    [DisallowMultipleComponent]
    public abstract class ObjectDataProvider<T> : MonoBehaviour, IDataProvider<T>
    {
        public abstract T GetData();
    }

    [DisallowMultipleComponent]
    public abstract class ObjectDataHandler<T> : MonoBehaviour, IDataHandler<T>
    {
        public abstract void HandleData(T data);
    }

    public abstract class ObjectDataInjector<T> : MonoBehaviour
    {
        [Header("數據提供者")]
        public ObjectDataProvider<T> DataProvider;

        public void InjectData(GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<ObjectDataHandler<T>>(out var handler))
            {
                Debug.LogError($"No ObjectDataHandler<{typeof(T).Name}> found on {targetGameObject.name}", targetGameObject);
                return;
            }
            handler.HandleData(DataProvider.GetData());
        }
    }
}