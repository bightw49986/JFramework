/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.Transformation
{
    /// <summary>
    /// 控制旋轉的基底類別，防止重複添加
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class RotationComponent : MonoBehaviour
    {
        // 這個基底類別用來避免在同一個物件上重複添加控制旋轉的組件
    }
}