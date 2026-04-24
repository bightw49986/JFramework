/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;
using JFramework.SOAP;

namespace JFramework.Editor
{
    /// <summary>
    /// 通用的 ScriptableVariable PropertyDrawer，支援所有類型
    /// </summary>
    
    [CustomPropertyDrawer(typeof(ScriptableVariable<>), true)]
    public class ScriptableVariablePropertyDrawer : PropertyDrawer
    {
        private const float SPACING = 4f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 檢查是否在 ValueReference 中使用（通過屬性路徑判斷）
            bool isInValueReference = property.propertyPath.Contains("scriptableValue");
            
            // 只有在不是 ValueReference 中使用時才繪製邊框
            if (!isInValueReference)
            {
                // 決定邊框顏色
                Color borderColor = property.objectReferenceValue != null ? 
                    new Color(0.6f, 0.6f, 0.6f, 0.8f) : // 灰色邊框（有對象）
                    new Color(1f, 0.4f, 0.4f, 0.9f);    // 紅色邊框（無對象）
                
                // 邊框往外擴展 3 像素，包圍整個內容
                Rect borderRect = new Rect(position.x - 3, position.y - 3, position.width + 6, position.height + 3);
                DrawBorder(borderRect, borderColor, 1f);
            }
            
            // 原本字段的高度和位置（完全不變，保持對齊）
            float originalHeight = EditorGUI.GetPropertyHeight(property, label, true);
            Rect originalRect = new Rect(position.x, position.y, position.width, originalHeight);
            
            // 使用原生方式繪製字段
            EditorGUI.PropertyField(originalRect, property, label, true);
            
            // 如果有對象，在下方展開其內容
            if (property.objectReferenceValue != null)
            {
                DrawExpandedContent(position, property.objectReferenceValue, originalRect.y + originalHeight);
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            
            // 如果有對象，添加展開內容的高度
            if (property.objectReferenceValue != null)
            {
                using (var serializedObject = new SerializedObject(property.objectReferenceValue))
                {
                    height += SPACING;
                    
                    var iterator = serializedObject.GetIterator();
                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            if (iterator.name == "m_Script" || iterator.name == "description") continue;
                            
                            height += EditorGUI.GetPropertyHeight(iterator, true) + 2f;
                        }
                        while (iterator.NextVisible(false));
                    }
                }
            }
            
            // 檢查是否在 ValueReference 中使用
            bool isInValueReference = property.propertyPath.Contains("scriptableValue");
            
            // 只有在不是 ValueReference 中使用時才為邊框添加額外空間
            if (!isInValueReference)
            {
                height += 4f; // 上下各2像素的邊距
            }
            
            return height;
        }
        
        /// <summary>
        /// 繪製邊框（只有邊框線，沒有填充）
        /// </summary>
        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            // 上邊框
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            // 下邊框
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            // 左邊框
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            // 右邊框
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }
        
        private void DrawExpandedContent(Rect position, Object target, float startY)
        {
            using (var serializedObject = new SerializedObject(target))
            {
                float currentY = startY + SPACING;
                string description = "";
                
                // 先找到 description 的值
                var descIterator = serializedObject.GetIterator();
                if (descIterator.NextVisible(true))
                {
                    do
                    {
                        if (descIterator.name == "description")
                        {
                            description = descIterator.stringValue;
                            break;
                        }
                    }
                    while (descIterator.NextVisible(false));
                }
                
                var iterator = serializedObject.GetIterator();
                if (iterator.NextVisible(true))
                {
                    EditorGUI.BeginChangeCheck();
                    
                    do
                    {
                        if (iterator.name == "m_Script") continue;
                        
                        // 跳過 Description 欄位，不單獨顯示
                        if (iterator.name == "description")
                        {
                            continue;
                        }
                        else
                        {
                            // 其他欄位繪製，顯示簡短描述
                            float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                            Rect propRect = new Rect(position.x, currentY, position.width, propHeight);
                            
                            // 創建顯示標籤
                            GUIContent labelContent;
                            if (string.IsNullOrEmpty(description))
                            {
                                // 沒有描述時，使用原本的標籤
                                labelContent = new GUIContent(iterator.displayName);
                            }
                            else
                            {
                                // 有描述時，顯示截斷的描述
                                string truncatedDescription = TruncateToSingleLine(description);
                                labelContent = new GUIContent(truncatedDescription);
                            }
                            
                            EditorGUI.PropertyField(propRect, iterator, labelContent, true);
                            
                            currentY += propHeight + 2f;
                        }
                    }
                    while (iterator.NextVisible(false));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }
        
        /// <summary>
        /// 將文字截斷到單行並加上省略號
        /// </summary>
        private string TruncateToSingleLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // 移除換行符號，用空格替代
            string singleLine = text.Replace('\n', ' ').Replace('\r', ' ');
            
            // 如果太長就截斷並加上省略號
            const int maxLength = 25; // 大約適合標籤顯示的長度
            if (singleLine.Length > maxLength)
            {
                return singleLine.Substring(0, maxLength) + "...";
            }
            
            return singleLine;
        }

    }
}