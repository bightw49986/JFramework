/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// GameObject物件池實作，繼承自泛型ObjectPoolBase
    /// </summary>
    public class GameObjectPool : ObjectPoolBase<Poolable, Poolable>
    {
        private readonly Transform poolContainer;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="prefab">要池化的Prefab</param>
        /// <param name="container">池容器</param>
        /// <param name="initialSize">初始大小</param>
        public GameObjectPool(Poolable prefab, Transform container, int initialSize = 10)
            : base(prefab, initialSize)
        {
            poolContainer = container;
            
            // 在子類別建構子完成後進行初始化
            Initialize();
        }

        /// <summary>
        /// 從池中取得物件（GameObject特化版本，支援位置和旋轉參數）
        /// </summary>
        /// <param name="position">物件位置</param>
        /// <param name="rotation">物件旋轉</param>
        /// <param name="parent">父物件</param>
        /// <returns>池化物件</returns>
        public Poolable Spawn(Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
        {
            bool setPosition = position.HasValue;
            bool setRotation = rotation.HasValue;

            return SpawnInternal(setPosition, position ?? Vector3.zero, setRotation, rotation ?? Quaternion.identity, parent);

            Poolable SpawnInternal(bool setPosition, Vector3 pos, bool setRotation, Quaternion rot, Transform parentTransform)
            {
                Poolable obj = Spawn((obj) =>
                {
                    // 只在指定時才設置位置
                    if (setPosition)
                    {
                        obj.transform.position = pos;
                    }

                    // 只在指定時才設置旋轉
                    if (setRotation)
                    {
                        obj.transform.rotation = rot;
                    }

                    obj.transform.SetParent(parentTransform);
                });

                return obj;
            }
        }

        #region ObjectPoolBase<GameObject> Abstract Methods Implementation
        protected override string PrototypeName => prototype != null ? prototype.name : "null";

        protected override Poolable CreateInstance(Poolable prototype)
        {
            return Object.Instantiate(prototype, poolContainer);
        }

        protected override void DestroyObject(Poolable obj)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        protected override void InternalOnSpawn(Poolable obj)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(true);
            }
        }

        protected override void InternalOnRecycle(Poolable obj)
        {
            if (obj != null)
            {
                obj.transform.SetParent(poolContainer);
                obj.gameObject.SetActive(false);
            }
        }

        protected override void InternalOnCreate(Poolable obj)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}