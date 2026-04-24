/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.Transformation
{
    /// <summary>
    /// 使物件持續旋轉
    /// </summary>
    public class Rotator : RotationComponent
    {
        [Header("旋轉設置")]
        [Tooltip("每秒旋轉角度 (度/秒)")]
        public Vector3 RotationSpeed = new Vector3(0, 90, 0);

        /// <summary>
        /// 每幀根據設定的旋轉速度進行旋轉
        /// 根據模式設定決定是否執行旋轉操作
        /// </summary>
        private void Update()
        {
            float deltaTime = Application.isPlaying ? Time.deltaTime : (1f / 60f);
            transform.Rotate(RotationSpeed * deltaTime);
        }
    }
}