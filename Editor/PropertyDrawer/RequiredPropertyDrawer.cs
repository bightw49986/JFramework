/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;

namespace JFramework.Editor
{
    /// <summary>
    /// Required屬性的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var requiredAttribute = (RequiredAttribute)attribute;
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            
            // 檢查是否需要顯示錯誤訊息
            if (IsPropertyEmpty(property))
            {
                var message = string.IsNullOrEmpty(requiredAttribute.Message) 
                    ? $"{label.text} is required" 
                    : requiredAttribute.Message;
                    
                var helpBoxHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth);
                height += helpBoxHeight + 2f; // 2f for spacing
            }
            
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var requiredAttribute = (RequiredAttribute)attribute;
            
            // 繪製原始屬性
            var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            var propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);
            
            EditorGUI.PropertyField(propertyRect, property, label, true);
            
            // 檢查是否為空並顯示錯誤訊息
            if (IsPropertyEmpty(property))
            {
                var message = string.IsNullOrEmpty(requiredAttribute.Message) 
                    ? $"{label.text} is required" 
                    : requiredAttribute.Message;
                
                var messageType = GetMessageType(requiredAttribute.MessageType);
                var helpBoxRect = new Rect(position.x, position.y + propertyHeight + 2f, position.width, 
                    position.height - propertyHeight - 2f);
                
                EditorGUI.HelpBox(helpBoxRect, message, messageType);
            }
        }

        private bool IsPropertyEmpty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue);
                case SerializedPropertyType.ArraySize:
                    return property.arraySize == 0;
                default:
                    return false;
            }
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
                    return MessageType.Error;
            }
        }
    }
}