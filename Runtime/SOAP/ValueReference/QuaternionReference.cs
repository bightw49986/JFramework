/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class QuaternionReference : ValueReference<Quaternion, ScriptableQuaternion>
    { 
        public QuaternionReference() { }
        public QuaternionReference(Quaternion constValue) : base(constValue) { }
    }
}
