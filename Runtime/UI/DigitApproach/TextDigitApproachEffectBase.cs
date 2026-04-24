/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.UI
{
    public abstract class TextDigitApproachEffectBase : TextEffectBase,
     IDataHandler<TextEffectContext<float>>,
     IDataHandler<TextEffectContext<int>>
    {
        [SerializeField]
        private FloatReference duration = new FloatReference(0.5f);

        [SerializeField]
        private FloatReference interval = new FloatReference(0.05f);

        [SerializeField]
        private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine effectCoroutine;
        protected string prefix;
        protected string suffix;
        private WaitForSeconds cachedWait;
        private float cachedWaitTime = -1f;

        public override void HandleData(TextEffectContext<string> context)
        {
            SetText(context.NewValue);
        }

        public void HandleData(TextEffectContext<float> data)
        {
            PlayDigitEffect(data.TextFormat, data.NumberFormat, data.NewValue, data.OldValue);
        }

        public void HandleData(TextEffectContext<int> data)
        {
            PlayDigitEffect(data.TextFormat, data.NumberFormat, data.NewValue, data.OldValue);
        }

        public void PlayDigitEffect(string textFormat, string numberFormat, float newValue, float oldValue)
        {
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
            }
            
            // Parse prefix and suffix once
            ParseFormat(textFormat);
            
            effectCoroutine = StartCoroutine(DigitApproachCoroutine(numberFormat, newValue, oldValue));

            IEnumerator DigitApproachCoroutine(string numberFormat, float newValue, float oldValue)
            {
                try
                {
                    if (duration.Value <= 0f)
                    {
                        Debug.LogWarning("[TextDigitApproachEffect]Duration must be greater than zero.");
                        yield break;
                    }
                    float totalDuration = duration.Value;
                    float stepInterval = interval.Value;
                    float elapsed = 0f;
                    float startValue = oldValue;
                    float endValue = newValue;
                    float lastValue = startValue;
                    bool atLeastSetOneTime = false;
                    
                    // Cache WaitForSeconds to avoid GC allocations
                    if (cachedWait == null || !Mathf.Approximately(cachedWaitTime, stepInterval))
                    {
                        cachedWait = new WaitForSeconds(stepInterval);
                        cachedWaitTime = stepInterval;
                    }
                    
                    while (elapsed < totalDuration)
                    {
                        float t = Mathf.Clamp01(elapsed / totalDuration);
                        float easedT = easingCurve.Evaluate(t);
                        float currentValue = Mathf.Lerp(startValue, endValue, easedT);
                        if (!atLeastSetOneTime ||!Mathf.Approximately(currentValue, lastValue))
                        {
                            SetFormattedText(currentValue, numberFormat);
                            lastValue = currentValue;
                            atLeastSetOneTime = true;
                        }
                        elapsed += stepInterval;
                        yield return cachedWait;
                    }

                    // 最終值
                    SetFormattedText(endValue, numberFormat);
                }
                finally
                {
                    effectCoroutine = null;
                    SetFormattedText(newValue, numberFormat);
                }
            }
        }

        private void ParseFormat(string textFormat)
        {
            var index = textFormat.IndexOf("{0}", System.StringComparison.Ordinal);
            if (index < 0)
            {
                prefix = textFormat;
                suffix = string.Empty;
            }
            else
            {
                prefix = textFormat.Substring(0, index);
                suffix = textFormat.Substring(index + 3);
            }
        }

        protected virtual void SetFormattedText(float value, string numberFormat)
        {
            string text = string.Concat(prefix, value.ToString(numberFormat), suffix);
            SetText(text);
        }

        protected abstract void SetText(string text);
    }
}