/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Collections;
using System.Text;
using JFramework.SOAP;
using UnityEngine;

namespace JFramework.UI
{
    public abstract class TypeWriterEffectBase : TextEffectBase
    {
        [Tooltip("The speed of the typewriter effect in characters per second.")]
        [SerializeField]
        private FloatReference charactersPerSecond = new FloatReference(25f);

        [SerializeField]
        private bool playEffectOnEnable = true;

        private string fullText;
        private Coroutine typingCoroutine;
        private readonly StringBuilder stringBuilder = new StringBuilder(256);
        private WaitForSeconds cachedWait;
        private float cachedWaitTime = -1f;

        protected abstract void SetText(string text);
        protected abstract string GetText();

        private void OnEnable()
        {
            if (playEffectOnEnable)
            {
                PlayTextEffect(GetText());
            }
        }

        private void OnDisable()
        {
            StopEffect();           
        }

        public void PlayTextEffect(string textToDisplay)
        {
            fullText = textToDisplay;
            SetText(string.Empty);
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeTextCoroutine());
        }

        public override void HandleData(TextEffectContext<string> context)
        {
            PlayTextEffect(context.NewValue);
        }

        public void StopEffect()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        private IEnumerator TypeTextCoroutine()
        {
            try
            {
                int currentCharIndex = 0;
                float interval = 1f / charactersPerSecond;
                int fullTextLength = fullText.Length;
                
                // Cache WaitForSeconds to avoid GC allocations
                if (cachedWait == null || !Mathf.Approximately(cachedWaitTime, interval))
                {
                    cachedWait = new WaitForSeconds(interval);
                    cachedWaitTime = interval;
                }
                
                stringBuilder.Clear();
                stringBuilder.EnsureCapacity(fullTextLength);
                
                while (currentCharIndex < fullTextLength)
                {
                    // Append one character at a time to avoid Substring allocations
                    stringBuilder.Append(fullText[currentCharIndex]);
                    currentCharIndex++;
                    SetText(stringBuilder.ToString());
                    yield return cachedWait;
                }
            }
            finally
            {
                SetText(fullText);
                typingCoroutine = null;
            }
        }
    }
}