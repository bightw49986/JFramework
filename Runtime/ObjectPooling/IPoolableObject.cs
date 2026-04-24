/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 可池化物件介面，定義可以被ObjectPool管理的物件基本行為
    /// </summary>
    public interface IPoolableObject
    {
        /// <summary>
        /// 物件請求被收回的事件
        /// </summary>
        event Action<IPoolableObject> RecycleRequested;

        /// <summary>
        /// 當物件從池中被取出時調用
        /// 用於重置物件狀態、啟用組件等初始化操作
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 當物件被回收到池中時調用
        /// 用於清理狀態、停用組件等收尾操作
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 是否正在被使用中
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 物件名稱，用於識別和除錯
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// 泛型可池化物件介面，支援任何類型的原型參考
    /// </summary>
    /// <typeparam name="T">原型物件類型</typeparam>
    public interface IPoolableObject<T> : IPoolableObject where T : class
    {
        /// <summary>
        /// 泛型原始原型參考，用於快速定位對應的物件池
        /// </summary>
        T Prototype { get; set; }
    }
}