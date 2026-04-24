/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.Utility;
using JFramework.SOAP;

namespace JFramework.UI
{
    public abstract class AlphaBreatherBase : MonoBehaviour
    {
        [SerializeField]
        private FloatReference breathePeriod = new FloatReference(1f);

        [SerializeField]
        private FloatReference minAlpha = new FloatReference(0f);

        [SerializeField]
        private FloatReference maxAlpha = new FloatReference(1f);

        private float timer;

        private void OnEnable()
        {
            timer = 0f;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // 使用MathUtility的SineWaveNormalized產生呼吸進度 (0~1)
            var t = MathUtility.SineWave01(1f / breathePeriod, timer);

            // 用Mathf.Lerp限制alpha在min/max之間
            var alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            SetAlpha(alpha);
        }

        protected abstract void SetAlpha(float alpha);
    }
}