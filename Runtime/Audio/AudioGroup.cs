/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace JFramework.Audio
{
    [Serializable]
    [CreateAssetMenu(menuName = "JFramework/Audio/AudioGroup")]
    public class AudioGroup : ScriptableObject
    {
        public List<AudioSet> AudioSets = new List<AudioSet>();
        public AudioGroupSettings Settings;
    }
}