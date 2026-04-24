/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;
using JFramework.SOAP;


namespace JFramework.Editor
{
    [CustomEditor(typeof(ScriptableVariable<>), true)]
    public class ScriptableVariableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            // Add Save button
            if (GUILayout.Button("Force Save"))
            {
                var targetObj = target as ScriptableObject;
                if (targetObj != null)
                {
                    EditorUtility.SetDirty(targetObj);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
