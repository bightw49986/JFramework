/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;

namespace JFramework.Editor
{
    [CustomPropertyDrawer(typeof(Vector2RangeAttribute))]
    public class Vector2RangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            // Create a regular Vector2 field
            Vector2 val = EditorGUI.Vector2Field(position, label, property.vector2Value);
            // If the value changed
            if (EditorGUI.EndChangeCheck())
            {
                var rangeAttribute = (Vector2RangeAttribute)attribute;
                // Clamp the X/Y values to be within the allowed range
                val.x = Mathf.Clamp(val.x, rangeAttribute.MinX, rangeAttribute.MaxX);
                val.y = Mathf.Clamp(val.y, rangeAttribute.MinY, rangeAttribute.MaxY);
                // Update the value of the property to the clampped value
                property.vector2Value = val;
            }
        }
    }

    [CustomPropertyDrawer(typeof(Vector3RangeAttribute))]
    public class Vector3RangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            // Create a regular Vector3 field
            Vector3 val = EditorGUI.Vector3Field(position, label, property.vector3Value);
            // If the value changed
            if (EditorGUI.EndChangeCheck())
            {
                var rangeAttribute = (Vector3RangeAttribute)attribute;
                // Clamp the X/Y/Z values to be within the allowed range
                val.x = Mathf.Clamp(val.x, rangeAttribute.MinX, rangeAttribute.MaxX);
                val.y = Mathf.Clamp(val.y, rangeAttribute.MinY, rangeAttribute.MaxY);
                val.z = Mathf.Clamp(val.z, rangeAttribute.MinZ, rangeAttribute.MaxZ);
                // Update the value of the property to the clampped value
                property.vector3Value = val;
            }
        }
    }
}