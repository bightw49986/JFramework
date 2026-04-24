/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.SOAP
{
    [Serializable]
    public class BoolReference : ValueReference<bool, ScriptableBool>
    {
        public BoolReference() { }
        public BoolReference(bool constValue) : base(constValue) { }
    }
}
