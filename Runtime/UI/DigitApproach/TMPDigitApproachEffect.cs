/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if TMP_PRESENT || TMP_IS_BUILTIN
using TMPro;
using UnityEngine;

namespace JFramework.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPDigitApproachEffect : TextDigitApproachEffectBase
    {
        [SerializeField]
        private TMP_Text targetText;

        protected override void SetFormattedText(float value, string numberFormat)
        {
            if (string.IsNullOrEmpty(numberFormat))
            {
                // Use TMP's efficient non-boxing formatting
                // prefix and suffix are parsed in base class
                // For now use string concat, but we could optimize further
                targetText.SetText(string.Concat(prefix, value.ToString(), suffix));
            }
            else
            {
                // With custom format, still use string concat to avoid boxing
                targetText.SetText(string.Concat(prefix, value.ToString(numberFormat), suffix));
            }
        }

        protected override void SetText(string text)
        {
            targetText.SetText(text);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (targetText == null)
            {
                targetText = GetComponent<TMP_Text>();
            }
        }
#endif
    }
}
#endif