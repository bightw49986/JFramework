/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.SOAP.Event;

namespace JFramework.Editor
{
    [CustomEditor(typeof(StaticEvent))]
    public class StaticEventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Add some space
            EditorGUILayout.Space();

            // Add a section header
            EditorGUILayout.LabelField("Test Event", EditorStyles.boldLabel);

            // Get the target StaticEvent
            StaticEvent staticEvent = (StaticEvent)target;

            // Create the test button
            if (GUILayout.Button("Raise"))
            {
                staticEvent.Raise(null);
                Debug.Log($"StaticEvent '{staticEvent.name}' raised with null sender");
            }

            // Add some help text
            EditorGUILayout.HelpBox("Click the button above to test the StaticEvent by calling Raise(null). Check the console for confirmation.", MessageType.Info);
        }
    }
}