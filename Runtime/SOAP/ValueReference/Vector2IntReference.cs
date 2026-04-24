/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class Vector2IntReference : ValueReference<Vector2Int, ScriptableVector2Int>
    {
        public Vector2IntReference() { }
        public Vector2IntReference(Vector2Int constValue) : base(constValue) { }
    }
}
