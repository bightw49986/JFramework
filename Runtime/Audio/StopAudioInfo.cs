/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using JFramework.SOAP;

namespace JFramework.Audio
{
    [System.Serializable]
    public struct StopAudioInfo
    {
        public StringReference GroupID;
        public float FadeOutDuration;
    }
}