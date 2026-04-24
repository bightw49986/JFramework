/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using JFramework.Binding;

namespace JFramework.Editor
{
    [CustomEditor(typeof(TextBinderBase), true)]
    [CanEditMultipleObjects]
    public class TextBinderBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw target text field (specific to the derived class)
            var targetTextProp = serializedObject.FindProperty("targetText");
            if (targetTextProp != null)
            {
                EditorGUILayout.PropertyField(targetTextProp);
            }
            
            // Draw bindingType
            var bindingTypeProp = serializedObject.FindProperty("bindingType");
            EditorGUILayout.PropertyField(bindingTypeProp);
            var bindingType = (BindingType)bindingTypeProp.enumValueIndex;
            
            // Draw bindingSource
            var bindingSourceProp = serializedObject.FindProperty("bindingSource");
            EditorGUILayout.PropertyField(bindingSourceProp);
            var bindingSource = (BindingSource)bindingSourceProp.enumValueIndex;

            // Draw only the relevant source field
            switch (bindingSource)
            {
                case BindingSource.String:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stringSource"));
                    break;
                case BindingSource.Int:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("intSource"));
                    break;
                case BindingSource.Float:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floatSource"));
                    break;
            }

            // Draw other fields
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textFormat"));
            if (bindingSource != BindingSource.String)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("numberFormat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("setTextAtEnable"));
            if (bindingType == BindingType.OnValueChanged)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textEffect"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
