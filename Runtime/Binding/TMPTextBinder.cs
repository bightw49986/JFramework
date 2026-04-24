/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if TMP_PRESENT || TMP_IS_BUILTIN
using TMPro;
using UnityEngine;
using JFramework.UI;

namespace JFramework.Binding
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPTextBinder : TextBinderBase
    {
        [Required]
        [SerializeField]
        private TMP_Text targetText;

        protected override void SetTextWithCurrentValue()
        {
            switch (bindingSource)
            {
                case BindingSource.String:
                    targetText.SetText(string.Concat(prefix, stringSource.Value, suffix));
                    break;
                case BindingSource.Int:
                    if (string.IsNullOrEmpty(numberFormat))
                    {
                        // Use TMP non-boxing formatting when no custom number format is specified
                        targetText.SetText(textFormat, intSource.Value);
                    }
                    else
                    {
                        // Pre-format the value and set the full text directly
                        targetText.SetText(string.Concat(prefix, intSource.Value.ToString(numberFormat), suffix));
                    }
                    break;
                case BindingSource.Float:
                    if (string.IsNullOrEmpty(numberFormat))
                    {
                        // Use TMP non-boxing formatting when no custom number format is specified
                        targetText.SetText(textFormat, floatSource.Value);
                    }
                    else
                    {
                        // Pre-format the value and set the full text directly
                        targetText.SetText(string.Concat(prefix, floatSource.Value.ToString(numberFormat), suffix));
                    }
                    break;
            }
        }

        protected override void OnFloatSourceValueChanged(float newValue, float oldValue)
        {
            if (textEffect != null && textEffect is IDataHandler<TextEffectContext<float>> floatDataHandler)
            {
                var effectContext = CreateTextEffectContext(newValue, oldValue);
                floatDataHandler.HandleData(effectContext);
                return;
            }
            
            if (string.IsNullOrEmpty(numberFormat))
            {
                // Use TMP non-boxing formatting when no custom number format is specified
                targetText.SetText(textFormat, newValue);
            }
            else
            {
                // Pre-format the value and set the full text directly
                targetText.SetText(string.Concat(prefix, newValue.ToString(numberFormat), suffix));
            }
        }

        protected override void OnIntSourceValueChanged(int newValue, int oldValue)
        {
            if (textEffect != null && textEffect is IDataHandler<TextEffectContext<int>> intDataHandler)
            {
                var effectContext = CreateTextEffectContext(newValue, oldValue);
                intDataHandler.HandleData(effectContext);
                return;
            }
            
            if (string.IsNullOrEmpty(numberFormat))
            {
                // Use TMP non-boxing formatting when no custom number format is specified
                targetText.SetText(textFormat, (float)newValue);
            }
            else
            {
                // Pre-format the value and set the full text directly
                targetText.SetText(string.Concat(prefix, newValue.ToString(numberFormat), suffix));
            }
        }

        protected override void SetTextDirectly(string text)
        {
            targetText.SetText(string.Concat(prefix, text, suffix));
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
