/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 物件池狀態資訊
    /// </summary>
    public readonly struct PoolStatus
    {
        /// <summary>
        /// 可用物件數量
        /// </summary>
        public int Available { get; }

        /// <summary>
        /// 總物件數量
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// 使用中的物件數量
        /// </summary>
        public int Active => Total - Available;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="available">可用數量</param>
        /// <param name="total">總數量</param>
        public PoolStatus(int available, int total)
        {
            Available = available;
            Total = total;
        }

        /// <summary>
        /// 轉換為字串表示
        /// </summary>
        public override string ToString()
        {
            return $"Available: {Available}, Total: {Total}, Active: {Active}";
        }

        /// <summary>
        /// 隱式轉換為 tuple
        /// </summary>
        public static implicit operator (int available, int total)(PoolStatus status)
        {
            return (status.Available, status.Total);
        }

        /// <summary>
        /// 從 tuple 隱式轉換
        /// </summary>
        public static implicit operator PoolStatus((int available, int total) tuple)
        {
            return new PoolStatus(tuple.available, tuple.total);
        }
    }

    /// <summary>
    /// 泛型物件池管理器基底類別
    /// </summary>
    /// <typeparam name="T">要池化的物件類型</typeparam>
    /// <typeparam name="U">原型物件類型</typeparam>
    /// <typeparam name="TPool">物件池類型</typeparam>
    public abstract class ObjectPoolManagerBase<T, U, TPool> : IDisposable
        where T : class, IPoolableObject<U>
        where U : class
        where TPool : ObjectPoolBase<T, U>
    {
        // 所有的物件池，以原型作為Key
        protected readonly Dictionary<U, TPool> pools = new Dictionary<U, TPool>();
        protected readonly int defaultPoolSize;
        private bool disposedValue;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="defaultPoolSize">預設池大小</param>
        /// <param name="coroutineRunner">協程執行器（可選）</param>
        protected ObjectPoolManagerBase(int defaultPoolSize = 10)
        {
            this.defaultPoolSize = defaultPoolSize;
        }

        /// <summary>
        /// 從物件池取得物件
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <returns>池化物件</returns>
        public T Spawn(U prototype)
        {
            if (prototype == null)
            {
                LogError("Prototype is null!");
                return null;
            }

            // 檢查是否已經有這個prototype的池
            if (!pools.ContainsKey(prototype))
            {
                CreatePool(prototype, defaultPoolSize);
            }

            var pool = pools[prototype];
            return pool.Spawn();
        }

        /// <summary>
        /// 將物件回收到物件池
        /// </summary>
        /// <param name="pooledObject">要回收的物件</param>
        public void Recycle(T pooledObject)
        {
            if (pooledObject == null)
            {
                LogError("Cannot recycle null object!");
                return;
            }

            var originalPrototype = pooledObject.Prototype;
            if (originalPrototype == null || !pools.ContainsKey(originalPrototype))
            {
                LogError($"No pool found for object {pooledObject.Name}! OriginalPrototype: {(originalPrototype != null ? GetPrototypeName(originalPrototype) : "null")}");
                return;
            }

            // 直接使用OriginalPrototype定位池，O(1)複雜度
            var targetPool = pools[originalPrototype];
            targetPool.Recycle(pooledObject);
        }

        /// <summary>
        /// 為指定prototype創建物件池
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <param name="initialSize">初始大小</param>
        public void CreatePool(U prototype, int initialSize = 0)
        {
            if (prototype == null)
            {
                LogError("Cannot create pool for null prototype!");
                return;
            }

            if (pools.ContainsKey(prototype))
            {
                LogWarning($"Pool for {GetPrototypeName(prototype)} already exists!");
                return;
            }

            // 檢查prototype是否有必要的組件
            if (prototype == null)
            {
                LogError($"Prototype {GetPrototypeName(prototype)} validation failed!");
                return;
            }

            var pool = CreatePoolInstance(prototype, initialSize > 0 ? initialSize : defaultPoolSize);
            pools[prototype] = pool;

            Log($"Created pool for {GetPrototypeName(prototype)} with initial size: {initialSize}");
        }

        /// <summary>
        /// 檢查指定prototype是否已有物件池
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <returns>是否存在池</returns>
        public bool HasPool(U prototype)
        {
            return prototype != null && pools.ContainsKey(prototype);
        }

        /// <summary>
        /// 清理所有池
        /// </summary>
        public void ClearAllPools()
        {
            if (disposedValue)
                return;

            foreach (var pool in pools.Values)
            {
                pool.Dispose();
            }
            pools.Clear();
        }

        /// <summary>
        /// 擴展指定prototype的池大小
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <param name="count">要增加的物件數量</param>
        public void Expand(U prototype, int count)
        {
            if (prototype == null)
            {
                LogError("Cannot expand pool for null prototype!");
                return;
            }

            if (count <= 0)
            {
                LogWarning("Expand count must be greater than 0!");
                return;
            }

            if (!pools.ContainsKey(prototype))
            {
                LogError($"No pool found for prototype {GetPrototypeName(prototype)}! Create pool first.");
                return;
            }

            pools[prototype].Expand(count);
        }

        #region Time-Sliced Warmup

        /// <summary>
        /// 執行時間分割預載入
        /// </summary>
        /// <param name="configs">預載入配置列表</param>
        public async Task ExecuteTimeSlicedWarmupAsync(IEnumerable<WarmupConfig<U>> configs)
        {
            await WarmupCoroutineAsync(configs);

            /// <summary>
            /// 預載入協程
            /// </summary>
            /// <param name="configs">配置列表</param>
            /// <returns></returns>
            async Task WarmupCoroutineAsync(IEnumerable<WarmupConfig<U>> configs)
            {
                var configList = new List<WarmupConfig<U>>(configs);
                Log($"Starting warmup process for {configList.Count} configurations");

                foreach (var config in configList)
                {
                    await WarmupSingleConfigAsync(config);
                }

                Log("Warmup process completed");

                /// <summary>
                /// 預載入單一配置的異步方法
                /// </summary>
                /// <param name="config">預載入配置</param>
                /// <returns></returns>
                async Task WarmupSingleConfigAsync(WarmupConfig<U> config)
                {
                    if (config.Prototype == null)
                    {
                        LogWarning("Skipping null prototype in warmup config");
                        return;
                    }

                    if (config.Count <= 0)
                    {
                        LogWarning($"Skipping warmup for {GetPrototypeName(config.Prototype)} - invalid count: {config.Count}");
                        return;
                    }

                    int framesCount = Mathf.Max(1, config.SplitIntoFrames);

                    Log($"Warming up {GetPrototypeName(config.Prototype)}: {config.Count} objects over {framesCount} frames");

                    // 確保池存在
                    if (!HasPool(config.Prototype))
                    {
                        CreatePool(config.Prototype, 0);
                    }

                    int totalFrames = 0;

                    // 當幀數大於物件數量時，需要平滑分散
                    if (framesCount >= config.Count)
                    {
                        // 計算每個物件之間的間隔幀數
                        float frameInterval = (float)framesCount / config.Count;

                        for (int objectIndex = 0; objectIndex < config.Count; objectIndex++)
                        {
                            // 計算應該在第幾幀建立這個物件
                            int targetFrame = Mathf.RoundToInt(objectIndex * frameInterval);

                            // 等待到目標幀
                            while (totalFrames < targetFrame)
                            {
                                await Task.Yield();
                                totalFrames++;
                            }

                            // 建立一個物件
                            Expand(config.Prototype, 1);

                            Log($"Frame {targetFrame + 1}/{framesCount}: Created 1 object for {GetPrototypeName(config.Prototype)} (Total: {objectIndex + 1}/{config.Count})");

                            await Task.Yield();
                            totalFrames++;
                        }

                        // 等待剩餘的幀數完成
                        while (totalFrames < framesCount)
                        {
                            await Task.Yield();
                            totalFrames++;
                        }
                    }
                    else
                    {
                        // 當物件數量大於幀數時，使用原本的邏輯
                        int objectsPerFrame = config.Count / framesCount;
                        int remainder = config.Count % framesCount;
                        int totalCreated = 0;

                        for (int frame = 0; frame < framesCount; frame++)
                        {
                            // 計算這個frame要建立的物件數量
                            int objectsThisFrame = objectsPerFrame;

                            // 將餘數分配到前面的frame
                            if (frame < remainder)
                            {
                                objectsThisFrame++;
                            }

                            if (objectsThisFrame > 0)
                            {
                                // 擴展池大小
                                Expand(config.Prototype, objectsThisFrame);
                                totalCreated += objectsThisFrame;

                                Log($"Frame {frame + 1}/{framesCount}: Created {objectsThisFrame} objects for {GetPrototypeName(config.Prototype)} (Total: {totalCreated}/{config.Count})");
                            }

                            // 等待下一個frame
                            await Task.Yield();
                        }
                    }

                    Log($"Completed warmup for {GetPrototypeName(config.Prototype)}: {config.Count} objects created over {framesCount} frames");
                }
            }
        }
        #endregion

        #region Abstract Methods

        /// <summary>
        /// 創建物件池實例
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <param name="initialSize">初始大小</param>
        /// <returns>物件池實例</returns>
        protected abstract TPool CreatePoolInstance(U prototype, int initialSize);

        /// <summary>
        /// 取得原型物件名稱
        /// </summary>
        /// <param name="prototype">原型物件</param>
        /// <returns>原型物件名稱</returns>
        protected abstract string GetPrototypeName(U prototype);
        #endregion

        #region Virtual Methods
        /// <summary>
        /// 記錄訊息
        /// </summary>
        /// <param name="message">訊息</param>
        protected virtual void Log(string message)
        {
            Debug.Log($"[ObjectPoolManager<{typeof(T).Name}>] {message}");
        }

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告訊息</param>
        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[ObjectPoolManager<{typeof(T).Name}>] {message}");
        }

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        protected virtual void LogError(string message)
        {
            Debug.LogError($"[ObjectPoolManager<{typeof(T).Name}>] {message}");
        }
        #endregion

        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearAllPools();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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