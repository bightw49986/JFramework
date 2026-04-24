/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.Utility;

namespace JFramework.Transformation
{
    /// <summary>
    /// 使物件以正弦波方式持續移動
    /// </summary>
    public class SineWaveMotion : MonoBehaviour
    {
        public enum MoveDirection
        {
            UpDown,     // 上下移動 (Y軸)
            LeftRight,  // 左右移動 (X軸)
            ForwardBack // 前後移動 (Z軸)
        }

        [Header("移動設定")]
        public MoveDirection moveDirection = MoveDirection.UpDown;

        [Min(0f)]
        public float amplitude = 2f; // 振幅（移動距離）
        [Min(0f)]
        public float frequency = 1f; // 頻率（移動速度）
        public float phaseOffset = 0f; // 相位偏移

        [Header("選項")]
        [SerializeField]
        private bool useUnscaledTime = false; // 是否使用不受時間縮放影響的時間


        private Vector3 startPosition; // 起始位置

        private void Awake()
        {
            // 記錄起始位置
            startPosition = transform.position;
        }

        private void OnEnable()
        {
            transform.position = startPosition;
        }

        private void Update()
        {
            UpdateWaveMotion();

            /// <summary>
            /// 更新正弦波移動
            /// </summary>
            void UpdateWaveMotion()
            {
                // 計算時間值
                float time = useUnscaledTime ? Time.unscaledTime : Time.time;

                // 計算正弦波值: sin(2π * frequency * time + phaseOffset)
                float sineValue = MathUtility.SineWave(frequency, time, phaseOffset);

                // 根據選擇的方向計算新位置
                Vector3 offset = Vector3.zero;

                switch (moveDirection)
                {
                    case MoveDirection.UpDown:
                        offset = Vector3.up * (sineValue * amplitude);
                        break;
                    case MoveDirection.LeftRight:
                        offset = Vector3.right * (sineValue * amplitude);
                        break;
                    case MoveDirection.ForwardBack:
                        offset = Vector3.forward * (sineValue * amplitude);
                        break;
                }

                // 設定新位置 = 起始位置 + 偏移量
                transform.position = startPosition + offset;
            }
        }

#if UNITY_EDITOR
        // 在場景視圖中顯示移動軌跡的輔助線
        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoStartPos = startPosition;

            // 設定Gizmo顏色
            Gizmos.color = Color.yellow;

            // 根據移動方向繪製軌跡線
            Vector3 directionVector = Vector3.zero;
            switch (moveDirection)
            {
                case MoveDirection.UpDown:
                    directionVector = Vector3.up;
                    break;
                case MoveDirection.LeftRight:
                    directionVector = Vector3.right;
                    break;
                case MoveDirection.ForwardBack:
                    directionVector = Vector3.forward;
                    break;
            }

            // 繪製移動範圍線段
            Vector3 maxPos = gizmoStartPos + directionVector * amplitude;
            Vector3 minPos = gizmoStartPos - directionVector * amplitude;

            Gizmos.DrawLine(minPos, maxPos);

            // 繪製起始位置
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(gizmoStartPos, 0.1f);

            // 繪製最大最小位置
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(maxPos, 0.08f);
            Gizmos.DrawSphere(minPos, 0.08f);
        }

        private void OnValidate()
        {
            startPosition = transform.position;
        }
#endif
    }
}
