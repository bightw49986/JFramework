/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System;

namespace JFramework.ObjectPooling
{
    /// <summary>
    /// 物件池預熱配置結構
    /// </summary>
    [Serializable]
    public struct WarmupConfig<U> where U : class
    {
        [Tooltip("要預載入的原型物件")]
        public U Prototype;

        [Min(1)]
        [Tooltip("要預先建立的物件數量")]
        public int Count;

        [Min(1)]
        [Tooltip("將建立過程分散到幾個Frame完成，有助於避免卡頓")]
        public int SplitIntoFrames;

        public WarmupConfig(U prototype, int count, int splitIntoFrames)
        {
            Prototype = prototype;
            Count = count;
            SplitIntoFrames = splitIntoFrames;
        }
    }

    /// <summary>
    /// GameObject專用的預熱配置，基於WarmupConfig<GameObject>
    /// </summary>
    [Serializable]
    public struct GameObjectWarmupConfig
    {
        [Tooltip("要預載入的Prefab")]
        public Poolable Prefab;

        [Min(1)]
        [Tooltip("要預先建立的物件數量")]
        public int Count;

        [Min(1)]
        [Tooltip("將建立過程分散到幾個Frame完成，有助於避免卡頓")]
        public int SplitIntoFrames;

        public GameObjectWarmupConfig(Poolable prefab, int count, int splitIntoFrames)
        {
            Prefab = prefab;
            Count = count;
            SplitIntoFrames = splitIntoFrames;
        }

        // 實作隱式轉換到基底類型
        public static implicit operator WarmupConfig<Poolable>(GameObjectWarmupConfig config)
        {
            return new WarmupConfig<Poolable>(config.Prefab, config.Count, config.SplitIntoFrames);
        }

        // 實作從基底類型的隱式轉換
        public static implicit operator GameObjectWarmupConfig(WarmupConfig<Poolable> config)
        {
            return new GameObjectWarmupConfig(config.Prototype, config.Count, config.SplitIntoFrames);
        }

    }
}