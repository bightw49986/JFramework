/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;

namespace JFramework.Editor
{
    /// <summary>
    /// ListDrawerSettings屬性的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(ListDrawerSettingsAttribute))]
    public class ListDrawerSettingsPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                var listSettings = (ListDrawerSettingsAttribute)attribute;
                
                // 如果設置為展開，計算所有元素的高度
                if (listSettings.Expanded && property.isExpanded)
                {
                    float height = EditorGUIUtility.singleLineHeight; // header
                    height += 2f; // spacing
                    
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        height += EditorGUI.GetPropertyHeight(element, true);
                        height += 2f; // spacing between elements
                    }
                    
                    height += EditorGUIUtility.singleLineHeight; // array size field
                    return height;
                }
            }
            
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                var listSettings = (ListDrawerSettingsAttribute)attribute;
                
                // 強制展開如果設置了Expanded
                if (listSettings.Expanded)
                {
                    property.isExpanded = true;
                }
                
                // 自定義標籤如果需要
                if (listSettings.ShowIndexLabels)
                {
                    DrawArrayWithIndexLabels(position, property, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private void DrawArrayWithIndexLabels(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 繪製陣列頭部
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                float currentY = headerRect.y + headerRect.height + 2f;
                
                // 陣列大小字段
                var sizeRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("Array.size"));
                currentY += EditorGUIUtility.singleLineHeight + 2f;
                
                // 繪製每個元素
                for (int i = 0; i < property.arraySize; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    var elementHeight = EditorGUI.GetPropertyHeight(element, true);
                    var elementRect = new Rect(position.x, currentY, position.width, elementHeight);
                    
                    var elementLabel = new GUIContent($"Element {i}");
                    EditorGUI.PropertyField(elementRect, element, elementLabel, true);
                    
                    currentY += elementHeight + 2f;
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }
    }
}