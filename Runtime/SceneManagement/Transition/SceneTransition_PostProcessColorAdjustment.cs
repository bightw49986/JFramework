/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if USE_URP
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace JFramework.SceneManagement
{
    [CreateAssetMenu(menuName = "JFramework/SceneTransition/PostProcessColorAdjustment", fileName = "SceneTransition_PostProcessColorAdjustment")]
    public class SceneTransition_PostProcessColorAdjustment : SceneTransitionBase
    {
        public float Duration = 1f;
        public bool SetStartColor = false;
        public Color StartColor = Color.white;
        public Color TargetColor = Color.black;

        public override async Task Perform()
        {
            var volume = FindObjectOfType<Volume>();

            if (!volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments = volume.profile.Add<ColorAdjustments>();
            }

            if (SetStartColor)
            {
                colorAdjustments.colorFilter.value = StartColor;
            }
            var currentColor = colorAdjustments.colorFilter.value;
            colorAdjustments.colorFilter.overrideState = true;

            float elapsed = 0f;
            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Duration);
                colorAdjustments.colorFilter.value = Color.Lerp(currentColor, TargetColor, t);
                await Task.Yield();
            }

            colorAdjustments.colorFilter.value = TargetColor;
        }
    }
}
#endif