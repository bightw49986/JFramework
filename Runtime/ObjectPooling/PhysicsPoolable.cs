/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Collections;
using UnityEngine;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 物理可池化物件組件，支援物理休眠檢測的自動回收功能
    /// </summary>
    public class PhysicsPoolable : Poolable
    {
        [SerializeField]
        [InfoBox("是否在Spawn時重置物理狀態")]
        private bool resetPhysicsOnSpawn = true;

        [SerializeField]
        [InfoBox("是否啟用休眠計時回收（物理物件休眠後開始計時）")]
        private bool enableSleepTimerRecycle = false;

        [ShowIf("enableSleepTimerRecycle")]
        [SerializeField]
        private float sleepRecycleTime = 3f;

        [SerializeField]
        [InfoBox("是否在碰撞時自動回收")]
        private bool enableCollisionRecycle = false;

        [ShowIf("enableCollisionRecycle")]
        [SerializeField]
        private LayerMask collisionRecycleLayers = -1; // 預設為所有 Layer

        [SerializeField]
        [InfoBox("是否在觸發器觸發時自動回收")]
        private bool enableTriggerRecycle = false;

        [ShowIf("enableTriggerRecycle")]
        [SerializeField]
        private LayerMask triggerRecycleLayers = -1; // 預設為所有 Layer

        // 快取組件參考
        private Rigidbody cachedRigidbody;
        private Rigidbody2D cachedRigidbody2D;

        // SleepTimer 相關
        private Coroutine sleepRecycleCoroutine;

        #region MonoBehaviour Lifecycle
        private void Awake()
        {
            // 快取物理組件參考
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedRigidbody2D = GetComponent<Rigidbody2D>();
        }
        #endregion

        #region Override Methods
        protected override void OnSpawnCustom()
        {
            // 重置物理狀態
            if (resetPhysicsOnSpawn)
            {
                ResetPhysics();
            }

            void ResetPhysics()
            {
                if (cachedRigidbody != null)
                {
                    cachedRigidbody.linearVelocity = Vector3.zero;
                    cachedRigidbody.angularVelocity = Vector3.zero;
                    cachedRigidbody.Sleep();
                }

                if (cachedRigidbody2D != null)
                {
                    cachedRigidbody2D.linearVelocity = Vector2.zero;
                    cachedRigidbody2D.angularVelocity = 0f;
                    cachedRigidbody2D.Sleep();
                }
            }
        }

        /// <summary>
        /// 啟動自動回收（覆寫父類方法以支援休眠計時）
        /// </summary>
        protected override void StartAutoRecycle()
        {
            // 先啟動基礎的定時回收
            base.StartAutoRecycle();

            // 啟動休眠計時回收
            if (enableSleepTimerRecycle && sleepRecycleTime > 0)
            {
                StartSleepTimerRecycle();
            }
        }

        /// <summary>
        /// 停止自動回收（覆寫父類方法以支援休眠計時）
        /// </summary>
        protected override void StopAutoRecycle()
        {
            // 停止基礎的定時回收
            base.StopAutoRecycle();

            // 停止休眠計時回收
            StopSleepTimerRecycle();
        }
        #endregion

        #region Collision and Trigger Events
        /// <summary>
        /// 3D 碰撞進入事件
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (enableCollisionRecycle && IsActive)
            {
                // 檢查碰撞物件的 Layer 是否在指定的 LayerMask 中
                if (IsLayerInMask(collision.gameObject.layer, collisionRecycleLayers))
                {
                    RecycleSelf();
                }
            }
        }

        /// <summary>
        /// 2D 碰撞進入事件
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (enableCollisionRecycle && IsActive)
            {
                // 檢查碰撞物件的 Layer 是否在指定的 LayerMask 中
                if (IsLayerInMask(collision.gameObject.layer, collisionRecycleLayers))
                {
                    RecycleSelf();
                }
            }
        }

        /// <summary>
        /// 3D 觸發器進入事件
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (enableTriggerRecycle && IsActive)
            {
                // 檢查觸發物件的 Layer 是否在指定的 LayerMask 中
                if (IsLayerInMask(other.gameObject.layer, triggerRecycleLayers))
                {
                    RecycleSelf();
                }
            }
        }

        /// <summary>
        /// 2D 觸發器進入事件
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (enableTriggerRecycle && IsActive)
            {
                // 檢查觸發物件的 Layer 是否在指定的 LayerMask 中
                if (IsLayerInMask(other.gameObject.layer, triggerRecycleLayers))
                {
                    RecycleSelf();
                }
            }
        }

        /// <summary>
        /// 檢查指定的 Layer 是否在 LayerMask 中
        /// </summary>
        /// <param name="layer">要檢查的 Layer</param>
        /// <param name="layerMask">LayerMask</param>
        /// <returns>如果 Layer 在 LayerMask 中則返回 true</returns>
        private bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
        #endregion

        #region Sleep Timer Methods
        /// <summary>
        /// 啟動休眠計時回收
        /// </summary>
        private void StartSleepTimerRecycle()
        {
            if (cachedRigidbody == null && cachedRigidbody2D == null)
            {
                Debug.LogWarning($"[PhysicsPoolable] {gameObject.name} 啟用休眠計時但沒有 Rigidbody 組件");
                return;
            }

            if (sleepRecycleCoroutine != null)
            {
                StopCoroutine(sleepRecycleCoroutine);
            }
            sleepRecycleCoroutine = StartCoroutine(SleepTimerRecycleCoroutine());
        }

        /// <summary>
        /// 停止休眠計時回收
        /// </summary>
        private void StopSleepTimerRecycle()
        {
            if (sleepRecycleCoroutine != null)
            {
                StopCoroutine(sleepRecycleCoroutine);
                sleepRecycleCoroutine = null;
            }
        }

        /// <summary>
        /// 休眠計時回收 Coroutine
        /// </summary>
        private IEnumerator SleepTimerRecycleCoroutine()
        {
            while (IsActive)
            {
                // 等待物理物件進入休眠狀態
                yield return new WaitUntil(() => IsRigidbodySleeping());

                // 物件休眠後，等待指定時間
                yield return new WaitForSeconds(sleepRecycleTime);

                // 如果物件仍在休眠且仍活躍，則回收
                if (IsActive && IsRigidbodySleeping())
                {
                    RecycleSelf();
                    yield break;
                }
            }
        }

        /// <summary>
        /// 檢查 Rigidbody 是否在休眠狀態
        /// </summary>
        /// <returns>true 如果有 Rigidbody 且在休眠狀態</returns>
        private bool IsRigidbodySleeping()
        {
            if (cachedRigidbody != null)
            {
                return cachedRigidbody.IsSleeping();
            }

            if (cachedRigidbody2D != null)
            {
                return cachedRigidbody2D.IsSleeping();
            }

            return false;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 如果啟用休眠計時但沒有 Rigidbody，發出警告
            if (enableSleepTimerRecycle)
            {
                var rb = GetComponent<Rigidbody>();
                var rb2D = GetComponent<Rigidbody2D>();
                if (rb == null && rb2D == null)
                {
                    Debug.LogWarning($"[PhysicsPoolable] {gameObject.name} 沒有 Rigidbody 組件");
                }
            }
        }
#endif
        #endregion
    }
}