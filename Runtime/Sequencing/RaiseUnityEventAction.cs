/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace JFramework.Sequencing
{
    public class RaiseUnityEventAction : SequenceAction
    {
        [SerializeField]
        private UnityEvent unityEvent;

        public override IEnumerator SequenceRoutine()
        {
            unityEvent?.Invoke();
            yield break;
        }
    }
}
