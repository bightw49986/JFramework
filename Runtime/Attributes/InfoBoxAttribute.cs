/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 在Inspector中顯示資訊框
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType InfoBoxType { get; }

        /// <summary>
        /// 創建資訊框屬性
        /// </summary>
        /// <param name="message">顯示的訊息</param>
        /// <param name="infoBoxType">資訊框類型</param>
        public InfoBoxAttribute(string message, InfoBoxType infoBoxType = InfoBoxType.Info)
        {
            Message = message;
            InfoBoxType = infoBoxType;
            order = -1; // 讓InfoBox顯示在字段前面
        }
    }

    /// <summary>
    /// 資訊框類型
    /// </summary>
    public enum InfoBoxType
    {
        /// <summary>普通資訊</summary>
        Info,
        /// <summary>警告</summary>
        Warning,
        /// <summary>錯誤</summary>
        Error,
        /// <summary>無圖示</summary>
        None
    }
}