/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if TMP_PRESENT || TMP_IS_BUILTIN
using TMPro;
using UnityEngine;

namespace JFramework.UI
{
    /// <summary>
    /// Makes the TextMeshPro alpha breathe in and out.
    /// </summary>
    public class TMPTextAlphaBreather : AlphaBreatherBase
    {
        [SerializeField]
        private TMP_Text text;

        protected override void SetAlpha(float alpha)
        {
            if (text != null)
            {
                var color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }
        }
#endif
    }
}
#endif