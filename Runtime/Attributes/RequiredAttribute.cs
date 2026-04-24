/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 標記字段為必填項目
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType MessageType { get; }

        /// <summary>
        /// 標記字段為必填
        /// </summary>
        /// <param name="message">當字段為空時顯示的訊息</param>
        /// <param name="messageType">訊息類型</param>
        public RequiredAttribute(string message = null, InfoBoxType messageType = InfoBoxType.Error)
        {
            Message = message;
            MessageType = messageType;
        }
    }
}