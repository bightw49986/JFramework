/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class Vector2Reference : ValueReference<Vector2, ScriptableVector2>
    {
        public Vector2Reference() { }
        public Vector2Reference(Vector2 constValue) : base(constValue) { }
    }
}
