/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP.Event;

namespace JFramework.Audio
{
    [CreateAssetMenu(menuName = "JFramework/Event/1 arg/PlayAudio", fileName = "PlayAudioEvent")]
    public class PlayAudioEvent : StaticEvent<PlayAudioInfo> { }
}