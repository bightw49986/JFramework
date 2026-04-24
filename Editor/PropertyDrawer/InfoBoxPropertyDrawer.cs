/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;

namespace JFramework.Editor
{
    /// <summary>
    /// InfoBox屬性的PropertyDrawer，支援基本的條件顯示
    /// 注意：由於DecoratorDrawer的限制，InfoBox無法直接讀取同一字段上的其他屬性
    /// 但可以通過InfoBox自己的條件參數來實現條件顯示
    /// </summary>
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxPropertyDrawer : DecoratorDrawer
    {
        private const float boxPadding = 6f;

        public override float GetHeight()
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;
            
            // 估算文字行數來計算高度，避免在GetHeight中調用GUI函數
            var messageLength = infoBoxAttribute.Message.Length;
            var estimatedLines = Mathf.Max(1, Mathf.CeilToInt(messageLength / 60f)); // 假設每行約60字符
            var lineHeight = EditorGUIUtility.singleLineHeight;
            
            return estimatedLines * lineHeight + boxPadding * 2;
        }

        public override void OnGUI(Rect position)
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;
            var messageType = GetMessageType(infoBoxAttribute.InfoBoxType);

            // 繪製HelpBox
            EditorGUI.HelpBox(position, infoBoxAttribute.Message, messageType);
        }

        private MessageType GetMessageType(InfoBoxType infoBoxType)
        {
            switch (infoBoxType)
            {
                case InfoBoxType.Error:
                    return MessageType.Error;
                case InfoBoxType.Warning:
                    return MessageType.Warning;
                case InfoBoxType.Info:
                    return MessageType.Info;
                case InfoBoxType.None:
                    return MessageType.None;
                default:
                    return MessageType.Info;
            }
        }
    }
}