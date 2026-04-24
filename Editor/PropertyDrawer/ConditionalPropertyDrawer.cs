/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace JFramework.Editor
{
    /// <summary>
    /// 條件顯示PropertyDrawer的基類
    /// </summary>
    public abstract class ConditionalPropertyDrawer : PropertyDrawer
    {
        protected bool ShouldShow(SerializedProperty property)
        {
            var conditionalAttribute = (ConditionalAttribute)attribute;
            
            // 首先嘗試在當前層級查找條件字段
            var conditionProperty = property.serializedObject.FindProperty(conditionalAttribute.ConditionField);
            
            // 如果在當前層級找不到，嘗試在相同父級查找（處理嵌套序列化）
            if (conditionProperty == null)
            {
                conditionProperty = FindPropertyInSameParent(property, conditionalAttribute.ConditionField);
            }
            
            if (conditionProperty == null)
            {
                // 如果通過序列化屬性找不到，嘗試反射方式
                var target = GetTargetObjectOfProperty(property);
                var conditionField = target.GetType().GetField(conditionalAttribute.ConditionField, 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                
                if (conditionField == null)
                {
                    // 嘗試獲取屬性
                    var conditionProp = target.GetType().GetProperty(conditionalAttribute.ConditionField,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    
                    if (conditionProp == null)
                    {
                        Debug.LogWarning($"Condition field '{conditionalAttribute.ConditionField}' not found on {target.GetType().Name}");
                        return true;
                    }
                    
                    var propertyValue = conditionProp.GetValue(target);
                    return EvaluateCondition(propertyValue, conditionalAttribute);
                }
                
                var fieldValue = conditionField.GetValue(target);
                return EvaluateCondition(fieldValue, conditionalAttribute);
            }
            
            // 使用SerializedProperty獲取值
            object value = GetSerializedPropertyValue(conditionProperty);
            return EvaluateCondition(value, conditionalAttribute);
        }
        
        /// <summary>
        /// 在相同父級中查找屬性（處理嵌套序列化物件）
        /// </summary>
        private SerializedProperty FindPropertyInSameParent(SerializedProperty property, string fieldName)
        {
            // 獲取當前屬性的路徑
            string propertyPath = property.propertyPath;
            
            // 查找最後一個點，獲取父級路徑
            int lastDotIndex = propertyPath.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                string parentPath = propertyPath.Substring(0, lastDotIndex);
                string targetPath = parentPath + "." + fieldName;
                return property.serializedObject.FindProperty(targetPath);
            }
            
            // 如果沒有父級，直接查找
            return property.serializedObject.FindProperty(fieldName);
        }
        
        /// <summary>
        /// 獲取SerializedProperty對應的實際物件
        /// </summary>
        private object GetTargetObjectOfProperty(SerializedProperty property)
        {
            object target = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Replace(".Array.data[", "[").Split('.');
            
            for (int i = 0; i < propertyNames.Length - 1; i++) // -1因為最後一個是當前屬性名
            {
                string propertyName = propertyNames[i];
                
                if (propertyName.Contains("["))
                {
                    // 處理陣列
                    string arrayName = propertyName.Substring(0, propertyName.IndexOf('['));
                    int index = int.Parse(propertyName.Substring(propertyName.IndexOf('[') + 1, propertyName.IndexOf(']') - propertyName.IndexOf('[') - 1));
                    
                    var arrayField = target.GetType().GetField(arrayName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (arrayField != null)
                    {
                        var arrayValue = arrayField.GetValue(target);
                        if (arrayValue is System.Collections.IList list)
                            target = list[index];
                    }
                }
                else
                {
                    // 處理一般字段
                    var field = target.GetType().GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                        target = field.GetValue(target);
                }
            }
            
            return target;
        }
        
        private object GetSerializedPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                default:
                    Debug.LogWarning($"Unsupported property type: {property.propertyType}");
                    return null;
            }
        }

        private bool EvaluateCondition(object fieldValue, ConditionalAttribute conditionalAttribute)
        {
            bool result = false;

            if (fieldValue == null && conditionalAttribute.CompareValue == null)
            {
                result = true;
            }
            else if (fieldValue != null)
            {
                if (conditionalAttribute.CompareValue == null)
                {
                    // 如果沒有指定比較值，檢查字段是否為真值
                    if (fieldValue is bool boolValue)
                        result = boolValue;
                    else
                        result = fieldValue != null;
                }
                else
                {
                    // 比較字段值與指定值
                    result = fieldValue.Equals(conditionalAttribute.CompareValue);
                }
            }

            return conditionalAttribute.Invert ? !result : result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
                return 0f;
            
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
                return;
            
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    /// <summary>
    /// ShowIf屬性的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : ConditionalPropertyDrawer
    {
    }

    /// <summary>
    /// HideIf屬性的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfPropertyDrawer : ConditionalPropertyDrawer
    {
    }
}