/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 可池化物件組件，所有可以被ObjectPool管理的物件都需要這個組件
    /// </summary>
    [DisallowMultipleComponent]
    public class Poolable : MonoBehaviour, IPoolableObject<Poolable>
    {
        public UnityEvent<GameObject> OnSpawnEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnRecycleEvent = new UnityEvent<GameObject>();

        public event System.Action<IPoolableObject> RecycleRequested;

        #region Serialized Fields

        [SerializeField]
        [InfoBox("是否在Spawn時重置Transform")]
        private bool resetTransformOnSpawn = true;

        [ShowIf("resetTransformOnSpawn")]
        [SerializeField]
        private bool detachParentOnTransformReset = true;

        [SerializeField]
        [InfoBox("是否啟用自動計時回收")]
        private bool enableAutoRecycle = false;

        [ShowIf("enableAutoRecycle")]
        [SerializeField]
        [Min(0)]
        private float autoRecycleTime = 5f;
        #endregion

        #region Private Fields
        // 初始Transform狀態
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;

        // 自動回收相關
        private Coroutine autoRecycleCoroutine;
        #endregion

        #region Properties
        /// <summary>
        /// 是否正在被使用中
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 原始Prefab參考，用於快速定位對應的物件池
        /// </summary>
        public Poolable Prototype { get; set; }
        #endregion

        #region MonoBehaviour Lifecycle
        private void Awake()
        {
            // 記錄初始Transform狀態
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            initialScale = transform.localScale;
        }
        #endregion

        #region IPoolableObject
        string IPoolableObject.Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }
        /// <summary>
        /// 當物件從池中被取出時調用
        /// </summary>
        public void OnSpawn()
        {
            IsActive = true;

            // 重置Transform
            if (resetTransformOnSpawn)
            {
                transform.SetLocalPositionAndRotation(initialPosition, initialRotation);
                transform.localScale = initialScale;

                if (detachParentOnTransformReset)
                {
                    transform.SetParent(null);
                }
            }

            // 啟動自動回收
            if (enableAutoRecycle && autoRecycleTime > 0)
            {
                StartAutoRecycle();
            }

            // 觸發自定義Spawn事件
            OnSpawnCustom();

            // 觸發UnityEvent事件
            OnSpawnEvent.Invoke(gameObject);
        }

        /// <summary>
        /// 當物件被回收到池中時調用
        /// </summary>
        public void OnRecycle()
        {
            IsActive = false;
            StopAutoRecycle();

            // 觸發自定義Recycle事件
            OnRecycleCustom();

            // 觸發UnityEvent事件
            OnRecycleEvent.Invoke(gameObject);
        }
        #endregion

        /// <summary>
        /// 將自己回收到物件池
        /// </summary>
        public void RecycleSelf()
        {
            if (!IsActive)
            {
                Debug.LogWarning($"[Poolable] {gameObject.name} 已經被回收，無法重複回收");
                return;
            }

            RecycleRequested?.Invoke(this);
        }

        /// <summary>
        /// 自定義Spawn行為，子類可以重寫
        /// </summary>
        protected virtual void OnSpawnCustom()
        {
            // 子類可以重寫這個方法來實現自定義的Spawn行為
        }

        /// <summary>
        /// 自定義Recycle行為，子類可以重寫
        /// </summary>
        protected virtual void OnRecycleCustom()
        {
            // 子類可以重寫這個方法來實現自定義的Recycle行為
        }

        /// <summary>
        /// 啟動自動回收
        /// </summary>
        protected virtual void StartAutoRecycle()
        {
            if (autoRecycleCoroutine != null)
            {
                StopCoroutine(autoRecycleCoroutine);
            }
            autoRecycleCoroutine = StartCoroutine(AutoRecycleCoroutine());

            /// <summary>
            /// 自動回收 Coroutine
            /// </summary>
            IEnumerator AutoRecycleCoroutine()
            {
                yield return new WaitForSeconds(autoRecycleTime);
                RecycleSelf();
            }
        }

        /// <summary>
        /// 停止自動回收
        /// </summary>
        protected virtual void StopAutoRecycle()
        {
            if (autoRecycleCoroutine != null)
            {
                StopCoroutine(autoRecycleCoroutine);
                autoRecycleCoroutine = null;
            }
        }
    }
}