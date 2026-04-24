/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class Vector3Reference : ValueReference<Vector3, ScriptableVector3>
    {
        public Vector3Reference() { }
        public Vector3Reference(Vector3 constValue) : base(constValue) { }
    }
}
