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
    public struct AudioSet
    {
        public StringReference AudioID;
        public AudioClip AudioClip;
    }
}