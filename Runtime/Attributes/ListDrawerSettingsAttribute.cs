/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 自定義列表顯示設置
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ListDrawerSettingsAttribute : PropertyAttribute
    {
        public bool Expanded { get; set; } = false;
        public bool DraggableItems { get; set; } = true;
        public bool ShowIndexLabels { get; set; } = false;
        public string AddCopiesLastElement { get; set; } = null;

        /// <summary>
        /// 自定義列表顯示設置
        /// </summary>
        public ListDrawerSettingsAttribute()
        {
        }
    }
}