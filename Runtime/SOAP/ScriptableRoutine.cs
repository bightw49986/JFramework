/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Collections;

namespace JFramework.SOAP
{
    public abstract class ScriptableRoutine : ScriptableObject
    {
        public abstract IEnumerator Execute();
    }
}