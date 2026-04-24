/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// GameObject專用物件池管理器
    /// </summary>
    public class GameObjectPoolManager : ObjectPoolManagerBase<Poolable, Poolable, GameObjectPool>
    {
        private readonly Transform poolContainer;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="poolContainer">池容器</param>
        /// <param name="defaultPoolSize">預設池大小</param>
        public GameObjectPoolManager(Transform poolContainer, int defaultPoolSize = 10)
            : base(defaultPoolSize)
        {
            this.poolContainer = poolContainer;
        }

        /// <summary>
        /// 從物件池取得GameObject（支援位置、旋轉和父物件參數）
        /// </summary>
        /// <param name="prefab">要生成的Prefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋轉</param>
        /// <param name="parent">父物件Transform</param>
        /// <returns>池化物件</returns>
        public Poolable Spawn(Poolable prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
        {
            if (prefab == null)
            {
                LogError("Prefab is null!");
                return null;
            }

            // 檢查是否已經有這個prefab的池
            if (!pools.ContainsKey(prefab))
            {
                CreatePool(prefab, defaultPoolSize);
            }

            var pool = pools[prefab];
            return pool.Spawn(position, rotation, parent);
        }

        #region ObjectPoolManagerBase Implementation

        protected override GameObjectPool CreatePoolInstance(Poolable prototype, int initialSize)
        {
            return new GameObjectPool(prototype, poolContainer, initialSize);
        }

        protected override string GetPrototypeName(Poolable prototype)
        {
            return prototype != null ? prototype.name : "null";
        }
        #endregion

        #region Generic Spawn Methods
        /// <summary>
        /// 從物件池取得物件並返回指定組件類型（完整參數）
        /// </summary>
        /// <typeparam name="T">組件類型</typeparam>
        /// <param name="prefab">要生成的Prefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋轉</param>
        /// <param name="parent">父物件Transform</param>
        /// <returns>池化物件的指定組件</returns>
        public T Spawn<T>(Poolable prefab, Vector3? position, Quaternion? rotation, Transform parent = null) where T : Component
        {
            var spawnedObject = Spawn(prefab, position, rotation, parent);
            return GetComponentFromSpawnedObject(spawnedObject.gameObject);

            T GetComponentFromSpawnedObject(GameObject spawnedObject)
            {
                if (spawnedObject == null)
                    return null;

                if (!spawnedObject.TryGetComponent<T>(out var component))
                {
                    LogError($"Component {typeof(T).Name} not found on spawned object {spawnedObject.name}");
                }
                return component;
            }
        }

        #endregion

        /// <summary>
        /// 記錄訊息
        /// </summary>
        /// <param name="message">訊息</param>
        protected override void Log(string message)
        {
            Debug.Log($"[GameObjectPool] {message}");
        }

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告訊息</param>
        protected override void LogWarning(string message)
        {
            Debug.LogWarning($"[GameObjectPool] {message}");
        }

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        protected override void LogError(string message)
        {
            Debug.LogError($"[GameObjectPool] {message}");
        }
    }
}