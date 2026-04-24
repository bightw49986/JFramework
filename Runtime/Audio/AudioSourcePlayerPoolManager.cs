/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.ObjectPooling;

namespace JFramework.Audio
{
    /// <summary>
    /// AudioSourcePlayer池管理器
    /// </summary>
    public class AudioSourcePlayerPoolManager : ObjectPoolManagerBase<AudioSourcePlayer, GameObject, AudioSourcePlayerPool>
    {
        private readonly Transform container;
        
        public AudioSourcePlayerPoolManager(int defaultPoolSize = 10, Transform container = null) 
            : base(defaultPoolSize)
        {
            this.container = container;
        }
        
        protected override AudioSourcePlayerPool CreatePoolInstance(GameObject prototype, int size)
        {
            return new AudioSourcePlayerPool(prototype, size, container);
        }
        
        protected override string GetPrototypeName(GameObject prototype)
        {
            return prototype?.name ?? "Unknown";
        }
        
        protected override void LogError(string message)
        {
            Debug.LogError($"[AudioSourcePlayerPoolManager] {message}");
        }
        
        protected override void LogWarning(string message)
        {
            Debug.LogWarning($"[AudioSourcePlayerPoolManager] {message}");
        }
    }
}