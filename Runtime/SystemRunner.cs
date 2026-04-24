/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Collections.Generic;
using JFramework.SOAP.ModularSystem;

namespace JFramework
{
    public class SystemRunner : MonoBehaviour
    {
        private struct CustomIntervalData
        {
            public float interval;
            public float lastInvokeTime;
        }

        public static SystemRunner Instance
        {
            get
            {
                if (instance == null && !isQuitting)
                {
                    var runnerObject = new GameObject("SystemRunner");
                    instance = runnerObject.AddComponent<SystemRunner>();
                }
                return instance;
            }
        }
        private static SystemRunner instance;

        private event Action OnUpdateEvent;
        private event Action OnFixedUpdateEvent;
        private event Action OnLateUpdateEvent;

        private readonly Dictionary<Action, CustomIntervalData> customIntervals = new Dictionary<Action, CustomIntervalData>();

        private readonly List<Action> toInvokeCache = new List<Action>();
        private readonly List<Action> toUpdateCache = new List<Action>();

        private SystemModule rootModule;
        private static bool isQuitting = false;

        #region MonoBehaviours
        private void Awake()
        {
            //Singleton
            var instances = FindObjectsByType<SystemRunner>();
            if (instances.Length > 1)
            {
                DestroyImmediate(this);
            }
            else
            {
                instance = this;
                rootModule = ScriptableObject.CreateInstance<SystemModule>();
                isQuitting = false;
                Application.quitting += OnQuit;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnQuit()
        {
            isQuitting = true;
        }
        
        private void OnDestroy()
        {
            Application.quitting -= OnQuit;
            isQuitting = true;
        }

        private void Update()
        {
            rootModule.Traverse((module) => { module.OnUpdate(Time.deltaTime); });
            OnUpdateEvent?.Invoke();
            ProcessCustomIntervals();

            void ProcessCustomIntervals()
            {
                var currentTime = Time.time;

                toInvokeCache.Clear();
                toUpdateCache.Clear();

                foreach (var kvp in customIntervals)
                {
                    var action = kvp.Key;
                    var data = kvp.Value;

                    if (currentTime - data.lastInvokeTime >= data.interval)
                    {
                        toInvokeCache.Add(action);
                        toUpdateCache.Add(action);
                    }
                }

                for (int i = 0; i < toUpdateCache.Count; i++)
                {
                    var action = toUpdateCache[i];
                    if (customIntervals.TryGetValue(action, out var data))
                    {
                        var updatedData = data;
                        updatedData.lastInvokeTime = currentTime;
                        customIntervals[action] = updatedData;
                    }
                }

                for (int i = 0; i < toInvokeCache.Count; i++)
                {
                    toInvokeCache[i]?.Invoke();
                }
            }
        }

        private void FixedUpdate()
        {
            OnFixedUpdateEvent?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdateEvent?.Invoke();
        }
        #endregion

        #region System Registry Methods
        /// <summary>
        /// 新增子系統並初始化
        /// </summary>
        /// <param name="module"></param>
        public void AddChildModule(SystemModule module, GameObject objectRoot)
        {
            rootModule.AddChild(module);
            module.Traverse((module) => { module.Initialize(objectRoot); });
        }
        /// <summary>
        /// 移除子系統並終止
        /// </summary>
        /// <param name="module"></param>
        public void RemoveChildModule(SystemModule module)
        {
            module.Traverse((module) => { module.Terminate(); });
            rootModule.RemoveChild(module);
        }
        #endregion

        #region Update Subscription Methods
        /// <summary>
        /// 訂閱 Update 事件
        /// </summary>
        public IUpdateSubscription SubscribeToUpdate(Action callback)
        {
            OnUpdateEvent += callback;
            return new UpdateSubscription(() => OnUpdateEvent -= callback);
        }

        /// <summary>
        /// 訂閱 FixedUpdate 事件
        /// </summary>
        public IUpdateSubscription SubscribeToFixedUpdate(Action callback)
        {
            OnFixedUpdateEvent += callback;
            return new UpdateSubscription(() => OnFixedUpdateEvent -= callback);
        }

        /// <summary>
        /// 訂閱 LateUpdate 事件
        /// </summary>
        public IUpdateSubscription SubscribeToLateUpdate(Action callback)
        {
            OnLateUpdateEvent += callback;
            return new UpdateSubscription(() => OnLateUpdateEvent -= callback);
        }

        /// <summary>
        /// 訂閱自定義間隔事件
        /// </summary>
        public IUpdateSubscription SubscribeToCustomInterval(Action callback, float interval)
        {
            customIntervals[callback] = new CustomIntervalData
            {
                interval = interval,
                lastInvokeTime = Time.time
            };

            return new UpdateSubscription(() => customIntervals.Remove(callback));
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// 獲取訂閱狀態資訊
        /// </summary>
        public string GetSubscriptionStatus()
        {
            var updateCount = OnUpdateEvent?.GetInvocationList().Length ?? 0;
            var fixedUpdateCount = OnFixedUpdateEvent?.GetInvocationList().Length ?? 0;
            var lateUpdateCount = OnLateUpdateEvent?.GetInvocationList().Length ?? 0;
            var customIntervalCount = customIntervals.Count;


            return $"Update: {updateCount} subscribers\n" +
                   $"FixedUpdate: {fixedUpdateCount} subscribers\n" +
                   $"LateUpdate: {lateUpdateCount} subscribers\n" +
                   $"CustomInterval: {customIntervalCount} subscribers";
        }

        public IEnumerable<INode<SystemModule>> GetAllModules()
        {
            return rootModule.Flatten();
        }
        #endregion

        /// <summary>
        /// Update 訂閱介面
        /// </summary>
        public interface IUpdateSubscription : IDisposable
        {
        }

        /// <summary>
        /// Update 訂閱實作
        /// </summary>
        internal class UpdateSubscription : IUpdateSubscription
        {
            private readonly Action unsubscribeAction;
            private bool isDisposed;

            public UpdateSubscription(Action unsubscribeAction)
            {
                this.unsubscribeAction = unsubscribeAction;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    unsubscribeAction?.Invoke();
                    isDisposed = true;
                }
            }
        }
    }
}
