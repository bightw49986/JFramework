/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 泛型物件池基底類別，支援任何類型的物件池化
    /// </summary>
    /// <typeparam name="T">要池化的物件類型</typeparam>
    /// <typeparam name="U">用來當作key的物件原型類型</typeparam>
    public abstract class ObjectPoolBase<T, U> : IDisposable
        where T : class, IPoolableObject<U>
        where U : class
    {
        /// <summary>
        /// 可用物件數量
        /// </summary>
        public int AvailableCount => availableObjects.Count;

        /// <summary>
        /// 總物件數量
        /// </summary>
        public int TotalCount => allObjects.Count;

        protected readonly U prototype;
        private readonly Queue<T> availableObjects = new();
        private readonly HashSet<T> allObjects = new(); // 用HashSet而不是List來加速Contains檢查，O(1) vs O(n)
        private readonly int initialSize;

        private bool isInitialized = false;
        private bool disposedValue;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="prototype">要池化的原型物件</param>
        /// <param name="initialSize">初始大小</param>
        protected ObjectPoolBase(U prototype, int initialSize = 10)
        {
            this.prototype = prototype;
            this.initialSize = initialSize;
        }

        /// <summary>
        /// 初始化物件池，應在子類別建構子完成後調用
        /// </summary>
        protected void Initialize()
        {
            if (isInitialized)
            {
                LogWarning("ObjectPool already initialized!");
                return;
            }

            // 預先建立指定數量的物件
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }

            isInitialized = true;
            Log($"Initialized pool with {initialSize} objects");
        }

        /// <summary>
        /// 從池中取得物件
        /// </summary>
        /// <returns>池化物件</returns>
        public T Spawn(Action<T> onBeforeSpawn = null)
        {
            T obj;

            // 如果沒有可用物件，建立新的（CreateNewObject 會自動入隊）
            if (availableObjects.Count == 0)
            {
                CreateNewObject();
            }

            obj = availableObjects.Dequeue();

            onBeforeSpawn?.Invoke(obj);

            // 執行物件啟用邏輯
            InternalOnSpawn(obj);

            // 通知IPoolableObject組件物件被取出
            obj.OnSpawn();

            return obj;
        }

        /// <summary>
        /// 回收物件到池中
        /// </summary>
        /// <param name="obj">要回收的物件</param>
        public void Recycle(T obj)
        {
            if (obj == null)
            {
                LogError("Cannot recycle null object!");
                return;
            }

            // 檢查是否屬於這個池
            if (!allObjects.Contains(obj))
            {
                LogError($"Object {obj.Name} doesn't belong to this pool!");
                return;
            }

            // 檢查是否已經在可用佇列中
            if (!obj.IsActive)
            {
                LogWarning($"Object {obj.Name} is already in the pool!");
                return;
            }

            // 通知IPoolableObject組件物件被回收
            obj.OnRecycle();

            // 執行物件回收邏輯
            InternalOnRecycle(obj);

            // 加入可用佇列
            availableObjects.Enqueue(obj);
        }

        /// <summary>
        /// 建立新的物件
        /// </summary>
        /// <returns>新建立的物件</returns>
        private T CreateNewObject()
        {
            T obj = CreateInstance(prototype);

            // 設定OriginalPrefab參考
            obj.Prototype = prototype;
            // 訂閱 RecycleRequested 事件
            obj.RecycleRequested += OnObjectRequestedRecycle;

            // 執行物件初始化邏輯
            InternalOnCreate(obj);
            // 加入管理清單
            allObjects.Add(obj);
            availableObjects.Enqueue(obj);

            // 設定名稱以便識別
            obj.Name = $"{PrototypeName} (Pool #{allObjects.Count})";

            return obj;
        }

        private void OnObjectRequestedRecycle(IPoolableObject obj)
        {
            if (obj is not T pooledTypeObj)
            {
                LogError("Received NotifyRecycle for incompatible object type!");
                return;
            }
            Recycle(pooledTypeObj);
        }

        /// <summary>
        /// 擴展池大小
        /// </summary>
        /// <param name="count">要增加的物件數量</param>
        public void Expand(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewObject();
            }

            Log($"Expanded pool by {count} objects. Total: {TotalCount}");
        }

        /// <summary>
        /// 清理池中所有物件
        /// </summary>
        public void Clear()
        {
            if (disposedValue)
                return;

            // 銷毀所有物件
            foreach (var obj in allObjects)
            {
                if (obj != null)
                {
                    obj.RecycleRequested -= OnObjectRequestedRecycle;
                    DestroyObject(obj);
                }
            }

            // 清空容器
            availableObjects.Clear();
            allObjects.Clear();

            Log($"Cleared pool");
        }

        #region Abstract Methods - 子類必須實作
        /// <summary>
        /// 取得原型物件名稱
        /// </summary>
        /// <returns>原型物件名稱</returns>
        protected abstract string PrototypeName { get; }

        /// <summary>
        /// 創建物件實例
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <returns>新的物件實例</returns>
        protected abstract T CreateInstance(U prototype);

        /// <summary>
        /// 銷毀物件
        /// </summary>
        /// <param name="obj">要銷毀的物件</param>
        protected abstract void DestroyObject(T obj);

        /// <summary>
        /// 物件被創建時的物件池內部處理邏輯
        /// </summary>
        /// <param name="obj">被創建的物件</param>
        protected virtual void InternalOnCreate(T obj) { }

        /// <summary>
        /// 物件被取出時的物件池內部處理邏輯
        /// </summary>
        /// <param name="obj">被取出的物件</param>
        protected virtual void InternalOnSpawn(T obj) { }

        /// <summary>
        /// 物件被回收時的物件池內部處理邏輯
        /// </summary>
        /// <param name="obj">被回收的物件</param>
        protected virtual void InternalOnRecycle(T obj) { }
        #endregion

        #region Virtual Methods - 子類可以選擇性重寫

        /// <summary>
        /// 記錄訊息
        /// </summary>
        /// <param name="message">訊息</param>
        protected virtual void Log(string message)
        {
            Debug.Log($"[ObjectPool<{PrototypeName}>] {message}");
        }

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告訊息</param>
        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[ObjectPool<{PrototypeName}>] {message}");
        }

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        protected virtual void LogError(string message)
        {
            Debug.LogError($"[ObjectPool<{PrototypeName}>] {message}");
        }
        #endregion

        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Clear();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}