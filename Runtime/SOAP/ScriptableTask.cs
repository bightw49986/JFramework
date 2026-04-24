/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Threading.Tasks;

namespace JFramework.SOAP
{
    public abstract class ScriptableTask : ScriptableObject
    {
        public abstract Task Execute();
    }
}