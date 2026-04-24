/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.SOAP
{
    [Serializable]
    public class FloatReference : ValueReference<float, ScriptableFloat>
    {
        public FloatReference() { }
        public FloatReference(float constValue) : base(constValue) { }
    }
}
