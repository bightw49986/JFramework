/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.UI;

namespace JFramework.UI
{

    /// <summary>
    /// Makes the text alpha breathe in and out.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextAlphaBreather : AlphaBreatherBase
    {
        [Required]
        [SerializeField]
        private Text text;

        protected override void SetAlpha(float alpha)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            text = GetComponent<Text>();
        }
#endif
    }
}