/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using JFramework.SOAP.ModularSystem;


namespace JFramework.ObjectPooling
{
    [CreateAssetMenu(menuName = "JFramework/System/ObjectPool")]
    public class ObjectPoolModule : SystemModule
    {
        [InfoBox("預設物件池初始化大小")]
        [SerializeField]
        private int defaultPoolSize = 0;

        [InfoBox("物件池初始化時的預載入設定")]
        [SerializeField]
        private List<GameObjectWarmupConfig> warmupConfigs = new List<GameObjectWarmupConfig>();

        // 物件池容器
        private Transform poolContainer;

        // 使用ObjectPoolManager管理所有池
        private GameObjectPoolManager gameObjectPoolManager;

        // 物件池容器的參考
        private GameObject poolContainerObject;

        public override void Initialize(GameObject objectRoot)
        {
            base.Initialize(objectRoot);

            // 創建物件池容器
            if (poolContainer == null)
            {
                poolContainerObject = new GameObject("ObjectPoolContainer");
                poolContainer = poolContainerObject.transform;
                poolContainer.SetParent(objectRoot.transform);
            }

            // 初始化ObjectPoolManager，傳入SystemRunner作為協程執行器
            gameObjectPoolManager = new GameObjectPoolManager(poolContainer, defaultPoolSize);

            // 開始執行時間分割預載入
            _ = StartWarmupProcessAsync();

            Debug.Log("[ObjectPoolModule] Initialized with default pool size: " + defaultPoolSize);
        }

        /// <summary>
        /// 開始時間分割預載入流程
        /// </summary>
        private async Task StartWarmupProcessAsync()
        {
            if (warmupConfigs.Count > 0)
            {
                // 利用隱式轉換，直接轉換為泛型配置
                var genericConfigs = new List<WarmupConfig<Poolable>>();
                foreach (var config in warmupConfigs)
                {
                    genericConfigs.Add(config); // 隱式轉換
                }

                // 使用ObjectPoolManagerBase的時間分割預載入功能的async版本
                await gameObjectPoolManager.ExecuteTimeSlicedWarmupAsync(genericConfigs);
            }
        }

        public override void Terminate()
        {
            // 清理所有池
            gameObjectPoolManager?.Dispose();

            // 銷毀容器物件
            if (poolContainerObject != null)
            {
                DestroyImmediate(poolContainerObject);
            }

            base.Terminate();
        }

        #region Public Service Methods
        /// <summary>
        /// 從物件池取得物件
        /// </summary>
        /// <param name="prefab">要生成的Prefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋轉</param>
        /// <param name="parent">父物件Transform</param>
        /// <returns>池化物件</returns>
        public Poolable Spawn(Poolable prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
        {
            return ValidateObjectConditions(prefab) ? gameObjectPoolManager.Spawn(prefab, position, rotation, parent) : null;
        }

        /// <summary>
        /// 從物件池取得物件並返回指定組件類型（完整參數）
        /// </summary>
        /// <typeparam name="T">組件類型</typeparam>
        /// <param name="prefab">要生成的Prefab</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋轉</param>
        /// <param name="parent">父物件Transform</param>
        /// <returns>池化物件的指定組件</returns>
        public T Spawn<T>(Poolable prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null) where T : Component
        {
            return ValidateObjectConditions(prefab) ? gameObjectPoolManager.Spawn<T>(prefab, position, rotation, parent) : null;
        }

        /// <summary>
        /// 將物件回收到物件池
        /// </summary>
        /// <param name="pooledObject">要回收的物件</param>
        public void Recycle(Poolable pooledObject)
        {
            if (!ValidateObjectConditions(pooledObject))
                return;

            gameObjectPoolManager.Recycle(pooledObject);
        }

        /// <summary>
        /// 創建物件池（如果尚未存在）並預載入指定數量的物件
        /// </summary>
        /// <param name="prefab">要生成的Prefab</param>
        /// <param name="initialSize">初始池大小</param>
        public void CreatePool(Poolable prefab, int initialSize = 0)
        {
            if (!ValidateObjectConditions(prefab))
                return;

            gameObjectPoolManager.CreatePool(prefab, initialSize);
        }
        #endregion

        /// <summary>
        /// 驗證物件操作的前置條件
        /// </summary>
        /// <param name="prefab">要檢查的 Prefab</param>
        /// <returns>是否通過驗證</returns>
        private bool ValidateObjectConditions(Poolable prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolModule] Cannot spawn null prefab!");
                return false;
            }

            if (gameObjectPoolManager == null)
            {
                Debug.LogError("[ObjectPoolModule] PoolManager not initialized!");
                return false;
            }

            return true;
        }
    }
}