/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.SOAP
{
    [Serializable]
    public class IntReference : ValueReference<int, ScriptableInt>
    {
        public IntReference() { }
        public IntReference(int constValue) : base(constValue) { }
    }
}
