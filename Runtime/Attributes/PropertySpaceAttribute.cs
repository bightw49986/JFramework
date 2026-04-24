/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 在屬性之間添加空間
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class PropertySpaceAttribute : PropertyAttribute
    {
        public float Space { get; }

        /// <summary>
        /// 添加屬性間距
        /// </summary>
        /// <param name="space">間距大小（像素）</param>
        public PropertySpaceAttribute(float space = 8f)
        {
            Space = space;
        }
    }
}