/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.UI;

namespace JFramework.UI
{
    [RequireComponent(typeof(Text))]
    public class TextTypeWriterEffect : TypeWriterEffectBase
    {
        [Tooltip("The Text component to apply the typewriter effect to.")]
        [SerializeField]
        private Text targetText;

        protected override void SetText(string text)
        {
            targetText.text = text;
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
                targetText = GetComponent<Text>();
            }
        }
#endif
    }
}