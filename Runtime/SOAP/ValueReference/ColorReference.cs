/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    [Serializable]
    public class ColorReference : ValueReference<Color, ScriptableColor>
    {
        public ColorReference() { }
        public ColorReference(Color constValue) : base(constValue) { }
    }
}
