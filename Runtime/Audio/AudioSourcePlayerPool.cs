/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.ObjectPooling;

namespace JFramework.Audio
{
    /// <summary>
    /// AudioSourcePlayer專用物件池
    /// </summary>
    public class AudioSourcePlayerPool : ObjectPoolBase<AudioSourcePlayer, GameObject>
    {
        private readonly Transform container;
        
        protected override string PrototypeName => prototype?.name ?? "AudioSourcePlayer";
        
        public AudioSourcePlayerPool(GameObject prototype, int initialSize = 10, Transform container = null) 
            : base(prototype, initialSize)
        {
            this.container = container;
            Initialize();
        }
        
        protected override AudioSourcePlayer CreateInstance(GameObject prototype)
        {
            var audioObject = Object.Instantiate(prototype);
            if (container != null)
                audioObject.transform.SetParent(container);
                
            if (!audioObject.TryGetComponent<AudioSourcePlayer>(out var player))
                player = audioObject.AddComponent<AudioSourcePlayer>();
                
            player.Prototype = prototype;
            return player;
        }
        
        protected override void InternalOnCreate(AudioSourcePlayer obj)
        {
            base.InternalOnCreate(obj);
            obj.RecycleRequested += OnObjectRequestRecycle;
        }
        
        protected override void DestroyObject(AudioSourcePlayer obj)
        {
            if (obj != null)
                obj.RecycleRequested -= OnObjectRequestRecycle;
                
            if (obj != null && obj.gameObject != null)
                Object.DestroyImmediate(obj.gameObject);
        }
        
        private void OnObjectRequestRecycle(IPoolableObject obj)
        {
            if (obj is AudioSourcePlayer player)
            {
                Recycle(player);
            }
        }
    }
}