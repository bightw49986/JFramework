/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class Vector3IntReference : ValueReference<Vector3Int, ScriptableVector3Int>
    {
        public Vector3IntReference() { }
        public Vector3IntReference(Vector3Int constValue) : base(constValue) { }
    }
}
