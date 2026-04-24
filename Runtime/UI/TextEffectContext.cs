/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿namespace JFramework.UI
{
    public readonly struct TextEffectContext<T>
    {
        public readonly string TextFormat;
        public readonly string NumberFormat;
        public readonly T NewValue;
        public readonly T OldValue;

        public TextEffectContext(string textFormat, string numberFormat, T newValue, T oldValue)
        {
            TextFormat = textFormat;
            NumberFormat = numberFormat;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}