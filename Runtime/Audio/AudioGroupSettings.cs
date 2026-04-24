/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using JFramework.SOAP;

namespace JFramework.Audio
{
    [Serializable]
    public struct AudioGroupSettings
    {
        public StringReference GroupID;
        
        [Range(0f, 1f)]
        public float MaxVolume;
        
        [Min(1)]
        public int MaxSimultaneousPlays;
        public AudioSwitchBehavior SwitchBehavior;
        
        [Header("3D Audio Settings")]
        public bool Enable3D;
        
        [Min(0f)]
        public float MinDistance;
        
        [Min(0f)]
        public float MaxDistance;
        public AudioRolloffMode RolloffMode;
        
        [Header("Volume Management")]
        [Tooltip("是否啟用平方根音量衰減")]
        public bool EnableVolumeDecay;
        
        [Range(0, 256)]
        [Tooltip("音效優先權 (0-256, 數值越低優先權越高)")]
        public int Priority;
    }
}