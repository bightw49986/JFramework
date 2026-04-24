/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.SOAP
{
    [Serializable]
    public class StringReference : ValueReference<string, ScriptableString>
    {
        public StringReference() { }
        public StringReference(string constValue) : base(constValue) { }
    }
}
