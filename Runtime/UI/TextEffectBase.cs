/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.UI
{
    public abstract class TextEffectBase : MonoBehaviour, IDataHandler<TextEffectContext<string>>
    {
        public abstract void HandleData(TextEffectContext<string> context);
    }
}