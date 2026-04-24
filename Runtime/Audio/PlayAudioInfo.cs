/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using JFramework.SOAP;
using UnityEngine;

namespace JFramework.Audio
{
    [System.Serializable]
    public struct PlayAudioInfo
    { 
        public StringReference AudioID;
        public StringReference GroupID;
        public float Volume;
        public float Pitch;
        public bool Loop;
        public float FadeInDuration;
        [Range(0f, 1f)]
        public float NormalizedTime;
    }
}