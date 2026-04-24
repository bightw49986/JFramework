/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Collections;
using UnityEngine;

namespace JFramework.Sequencing
{
    public abstract class SequenceAction : MonoBehaviour
    {
        public virtual void OnActionStart() { }
        public abstract IEnumerator SequenceRoutine();
        public virtual void OnActionEnd() { }
    }
}
