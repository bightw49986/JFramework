/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.SOAP;

namespace JFramework.Editor
{
    /// <summary>
    /// 通用的 ValueReference PropertyDrawer，支援緊湊的 UI 設計
    /// </summary>
    [CustomPropertyDrawer(typeof(ValueReference<,>), true)]
    public class ValueReferencePropertyDrawer : PropertyDrawer
    {
        private const float BUTTON_WIDTH = 60f;
        private const float SPACING = 2f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 找到相關的序列化屬性
            var useScriptableValueProp = property.FindPropertyRelative("useScriptableValue");
            var scriptableValueProp = property.FindPropertyRelative("scriptableValue");
            var constValueProp = property.FindPropertyRelative("constValue");
            
            // 如果使用 ScriptableVariable，繪製邊框（不管有沒有對象）
            if (useScriptableValueProp.boolValue)
            {
                // 決定邊框顏色
                Color borderColor = scriptableValueProp.objectReferenceValue != null ? 
                    new Color(0.6f, 0.6f, 0.6f, 0.8f) : // 灰色邊框（有對象）
                    new Color(1f, 0.4f, 0.4f, 0.9f);    // 紅色邊框（無對象）
                
                // 計算實際需要的邊框高度
                float actualContentHeight;
                if (scriptableValueProp.objectReferenceValue != null)
                {
                    // 有對象時：包圍整個內容
                    actualContentHeight = position.height - 4f; // 減去我們添加的邊框空間
                }
                else
                {
                    // 沒有對象時：只包圍第一行和 ScriptableVariable 選擇器行
                    actualContentHeight = EditorGUIUtility.singleLineHeight * 2 + SPACING;
                }
                
                // 邊框往外擴展 3 像素，包圍實際內容
                Rect borderRect = new Rect(position.x - 3, position.y - 3, position.width + 6, actualContentHeight + 6);
                DrawBorder(borderRect, borderColor, 1f);
            }
            
            // 計算各部分的位置
            float currentY = position.y;
            
            // 第一行：主標籤 + Button + 內容
            Rect firstLineRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            
            // 主標籤位置
            Rect labelRect;
            Rect buttonRect;
            string buttonText = useScriptableValueProp.boolValue ? "使用常數" : "連結變數";
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.fontSize = 10;
            buttonStyle.padding = new RectOffset(4, 4, 2, 2);

            if (useScriptableValueProp.boolValue)
            {
                // ScriptableVariable模式：按鈕貼齊最右邊，標籤佔左側
                labelRect = new Rect(position.x, currentY, position.width - BUTTON_WIDTH - SPACING, EditorGUIUtility.singleLineHeight);
                buttonRect = new Rect(position.x + position.width - BUTTON_WIDTH, currentY, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
            }
            else
            {
                // 常數模式：按鈕在標籤右側
                labelRect = new Rect(position.x, currentY, EditorGUIUtility.labelWidth - BUTTON_WIDTH - SPACING, EditorGUIUtility.singleLineHeight);
                buttonRect = new Rect(EditorGUIUtility.labelWidth - BUTTON_WIDTH, currentY, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
            }

            EditorGUI.LabelField(labelRect, label);
            if (GUI.Button(buttonRect, buttonText, buttonStyle))
            {
                useScriptableValueProp.boolValue = !useScriptableValueProp.boolValue;
            }
            
            // 內容區域位置
            Rect contentRect = new Rect(EditorGUIUtility.labelWidth, currentY, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            
            if (useScriptableValueProp.boolValue)
            {
                // 使用 ScriptableVariable：換行顯示，使用完整寬度
                currentY += EditorGUIUtility.singleLineHeight + SPACING;
                
                // ScriptableVariable 選擇器使用完整寬度
                Rect scriptableRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(scriptableRect, scriptableValueProp, GUIContent.none);
                
                // ScriptableVariable 的內容會由 ScriptableVariablePropertyDrawer 自動處理
                // 這裡不需要做任何事情
            }
            else
            {
                // 使用常數值：保持同一行
                EditorGUI.PropertyField(contentRect, constValueProp, GUIContent.none);
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var useScriptableValueProp = property.FindPropertyRelative("useScriptableValue");
            var scriptableValueProp = property.FindPropertyRelative("scriptableValue");
            var constValueProp = property.FindPropertyRelative("constValue");
            
            float height = EditorGUIUtility.singleLineHeight;
            
            if (useScriptableValueProp.boolValue)
            {
                // ScriptableVariable 模式：ScriptableVariable 的高度由其 PropertyDrawer 決定
                height += SPACING + EditorGUI.GetPropertyHeight(scriptableValueProp, true);
                
                // 如果使用 ScriptableVariable 模式，為邊框添加額外空間
                height += 4f; // 上下各2像素的邊距給邊框
            }
            else
            {
                // 常數值模式：如果常數值是多行的（如 Vector），需要額外高度
                float constHeight = EditorGUI.GetPropertyHeight(constValueProp, true);
                if (constHeight > EditorGUIUtility.singleLineHeight)
                {
                    height = constHeight;
                }
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

    }
}