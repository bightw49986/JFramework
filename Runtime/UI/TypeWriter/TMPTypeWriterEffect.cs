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
    public class TMPTypeWriterEffect : TypeWriterEffectBase
    {
        [Tooltip("The TextMeshProUGUI component to apply the typewriter effect to.")]
        [SerializeField]
        private TMP_Text targetText;

        protected override void SetText(string text)
        {
            targetText.SetText(text);
        }

        protected override string GetText()
        {
            return targetText.text;
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