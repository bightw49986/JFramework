/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.Utility
{
    /// <summary>
    /// 數學工具類，提供常用的數學計算方法
    /// </summary>
    public static class MathUtility
    {
        /// <summary>
        /// 標準正弦波，值域 [-1, 1]，當 t= 0, 正弦值為0
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <param name="phaseOffset">相位偏移，單位為弧度</param>
        /// <returns>-1 到 1 之間的正弦值</returns>
        public static float SineWave(float frequency, float time, float phaseOffset = 0f)
        {
            return Mathf.Sin(2f * Mathf.PI * frequency * time + phaseOffset);
        }

        /// <summary>
        /// 標準餘弦波，值域 [-1, 1]，當 t= 0, 餘弦值為1
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <param name="phaseOffset">相位偏移，單位為弧度</param>
        /// <returns>-1 到 1 之間的餘弦值</returns>
        public static float CosineWave(float frequency, float time, float phaseOffset = 0f)
        {
            return Mathf.Cos(2f * Mathf.PI * frequency * time + phaseOffset);
        }

        /// <summary>
        /// 乒乓波，值域 [0, 1]，在0和1之間來回振盪
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <param name="phaseOffset">相位偏移，單位為弧度</param>
        /// <returns>0 到 1 之間的乒乓波值</returns>
        public static float PingPongWave(float frequency, float time, float phaseOffset = 0f)
        {
            return Mathf.PingPong(GetCycle(frequency, time, phaseOffset) * 2f, 1f);
        }

        /// <summary>
        /// 鋸齒波，值域 [0, 1]，線性遞增後重置
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <param name="phaseOffset">相位偏移，單位為弧度</param>
        /// <returns>0 到 1 之間的鋸齒波值</returns>
        public static float SawtoothWave(float frequency, float time, float phaseOffset = 0f)
        {
            return Mathf.Repeat(GetCycle(frequency, time, phaseOffset), 1f);
        }

        /// <summary>
        /// 正規化正弦波，值域 [0, 1]，從 0 開始
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <returns>0 到 1 之間的正弦值，從 0 開始</returns>
        public static float SineWave01(float frequency, float time)
        {
            // 內建相位偏移 -π/2，讓波形從 0 開始
            float sineValue = SineWave(frequency, time, -Mathf.PI * 0.5f);
            return RemapMinusOneOneTo01(sineValue);
        }

        /// <summary>
        /// 反向正規化正弦波，值域 [1, 0]，從 1 開始
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <returns>1 到 0 之間的正弦值，從 1 開始</returns>
        public static float SineWave10(float frequency, float time)
        {
            return 1f - SineWave01(frequency, time);
        }

        /// <summary>
        /// 計算以週期為單位的進度值（可用於波形函式）
        /// </summary>
        /// <param name="frequency">頻率</param>
        /// <param name="time">時間</param>
        /// <param name="phaseOffset">相位偏移（弧度）</param>
        /// <returns>返回總週期數，可搭配 Mathf.Repeat 或 Mathf.PingPong 使用</returns>
        public static float GetCycle(float frequency, float time, float phaseOffset = 0f)
        {
            return frequency * time + phaseOffset / (2f * Mathf.PI);
        }

        /// <summary>
        /// 將 [0, 1] 範圍的值重新映射到 [-1, 1] 範圍
        /// </summary>
        /// <param name="value01">輸入值，範圍為 [0, 1]</param>
        /// <returns>映射後的值，範圍為 [-1, 1]</returns>
        public static float Remap01ToMinusOneOne(float value01)
        {
            return value01 * 2f - 1f;
        }

        /// <summary>
        /// 將 [-1, 1] 範圍的值重新映射到 [0, 1] 範圍
        /// </summary>
        /// <param name="valueMinusOneOne">輸入值，範圍為 [-1, 1]</param>
        /// <returns>映射後的值，範圍為 [0, 1]</returns>
        public static float RemapMinusOneOneTo01(float valueMinusOneOne)
        {
            return (valueMinusOneOne + 1f) * 0.5f;
        }

        /// <summary>
        /// 矩形角落枚舉
        /// </summary>
        public enum RectCorner
        {
            BottomLeft = 0,
            BottomRight = 1,
            TopRight = 2,
            TopLeft = 3
        }

        public static readonly Vector2 HalfVector2 = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// 估算單位正方形周長上的點
        /// </summary>
        /// <param name="t">進度：0 到 1 之間的值，在無偏移情況下，0 表示左下角、0.25 表示右下角、0.5 表示右上角、0.75 表示左上角</param>
        /// <param name="startCorner">從哪個角落開始</param>
        /// <param name="clockwise">是否順時針進行</param>
        public static Vector2 EvaluateSquarePerimeter(
            float t,
            RectCorner startCorner = RectCorner.BottomLeft,
            bool clockwise = false)
        {
            if (clockwise)
                t = 1f - t;

            float startOffset = 0.25f * (int)startCorner;
            t = Mathf.Repeat(t + startOffset, 1f);

            float u = t * 4f;
            int segment = Mathf.FloorToInt(u);
            float f = u - segment;

            return segment switch
            {
                0 => new Vector2(f, 0f),
                1 => new Vector2(1f, f),
                2 => new Vector2(1f - f, 1f),
                _ => new Vector2(0f, 1f - f),
            };
        }

        /// <summary>
        /// 估算矩形周長上的點（可旋轉）
        /// </summary>
        /// <param name="center">矩形中心點位置</param>
        /// <param name="size">矩形尺寸</param>
        /// <param name="t">進度：0 到 1 之間的值，在無偏移情況下，0 表示左下角、0.25 表示右下角、0.5 表示右上角、0.75 表示左上角</param>
        /// <param name="startCorner">從哪個角落開始</param>
        /// <param name="clockwise">是否順時針進行</param>
        /// <param name="rotateDeg">旋轉角度（度）</param>
        public static Vector2 EvaluateRectPerimeter(
            Vector2 center,
            Vector2 size,
            float t,
            RectCorner startCorner = RectCorner.BottomLeft,
            bool clockwise = false,
            float rotateDeg = 0f)
        {
            // 正規化角度，讓 360 的倍數視為 0
            rotateDeg = Mathf.Repeat(rotateDeg, 360f);

            var p01 = EvaluateSquarePerimeter(t, startCorner, clockwise);
            var local = Vector2.Scale(p01 - HalfVector2, size);

            if (Mathf.Approximately(rotateDeg, 0f))
                return center + local;

            float rad = rotateDeg * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            var rotated = new Vector2(local.x * c - local.y * s, local.x * s + local.y * c);
            return center + rotated;
        }

    }
}