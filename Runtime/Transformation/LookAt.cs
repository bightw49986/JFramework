/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.Transformation
{
    /// <summary>
    /// 使物件持續面向目標
    /// </summary>
    [ExecuteInEditMode]
    public class LookAt : RotationComponent
    {
        [Required]
        [Header("目標設置")]
        [Tooltip("目標 Transform，物件將面向此目標")]
        public Transform Target;

        [Tooltip("限制旋轉軸 (1=限制該軸, 0=允許該軸旋轉)")]
        [SerializeField]
        private Vector3 constrainAxis = Vector3.zero;

        [Tooltip("是否反轉面向方向")]
        [SerializeField]
        private bool invertRotation = false;
       
        /// <summary>
        /// 每幀檢查是否需要面向目標
        /// 根據模式設定決定是否執行面向操作
        /// </summary>
        private void Update()
        {
            if (Target == null)
                return;

            LookAtTargetWithConstraint();
        }

        private void LookAtTargetWithConstraint()
        {
            // 計算面向目標的旋轉
            Vector3 direction = Target.position - transform.position;
            if (invertRotation)
            {
                direction = -direction;
            }

            // 計算目標旋轉
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // 如果有軸限制，需要保持當前在被限制軸上的旋轉
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Vector3 targetEuler = targetRotation.eulerAngles;

            if (constrainAxis.x != 0) targetEuler.x = currentEuler.x;
            if (constrainAxis.y != 0) targetEuler.y = currentEuler.y;
            if (constrainAxis.z != 0) targetEuler.z = currentEuler.z;

            transform.rotation = Quaternion.Euler(targetEuler);
        }
    }
}