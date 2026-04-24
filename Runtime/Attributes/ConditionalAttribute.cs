/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 條件顯示屬性的基類
    /// </summary>
    public abstract class ConditionalAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public object CompareValue { get; }
        public bool Invert { get; }

        protected ConditionalAttribute(string conditionField, object compareValue = null, bool invert = false)
        {
            ConditionField = conditionField;
            CompareValue = compareValue ?? true;
            Invert = invert;
        }
    }

    /// <summary>
    /// 當條件為真時顯示字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ShowIfAttribute : ConditionalAttribute
    {
        /// <summary>
        /// 當指定字段為true時顯示
        /// </summary>
        /// <param name="conditionField">條件字段名稱</param>
        public ShowIfAttribute(string conditionField) : base(conditionField) { }

        /// <summary>
        /// 當指定字段等於特定值時顯示
        /// </summary>
        /// <param name="conditionField">條件字段名稱</param>
        /// <param name="compareValue">比較值</param>
        public ShowIfAttribute(string conditionField, object compareValue) : base(conditionField, compareValue) { }
    }

    /// <summary>
    /// 當條件為真時隱藏字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class HideIfAttribute : ConditionalAttribute
    {
        /// <summary>
        /// 當指定字段為true時隱藏
        /// </summary>
        /// <param name="conditionField">條件字段名稱</param>
        public HideIfAttribute(string conditionField) : base(conditionField, true, true) { }

        /// <summary>
        /// 當指定字段等於特定值時隱藏
        /// </summary>
        /// <param name="conditionField">條件字段名稱</param>
        /// <param name="compareValue">比較值</param>
        public HideIfAttribute(string conditionField, object compareValue) : base(conditionField, compareValue, true) { }
    }
}