/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.SOAP.Event;
using System.Reflection;
using System;

namespace JFramework.Editor
{
    /// <summary>
    /// Universal editor that automatically handles all StaticEvent<T> subclasses
    /// Uses the StaticEventBase class to target only StaticEvent types without affecting other ScriptableObjects
    /// </summary>
    [CustomEditor(typeof(GenericStaticEventBase), true)]
    public class GenericStaticEventEditor : UnityEditor.Editor
    {
        private MethodInfo raiseMethod;
        private FieldInfo testValueField;
        private Type genericEventType;
        private bool isStaticEventGeneric = false;

        void OnEnable()
        {
            // Check if the target inherits from StaticEvent<T>
            Type targetType = target.GetType();
            Type baseType = targetType.BaseType;

            // Check if this is a generic StaticEvent<T> or its subclass
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "StaticEvent`1")
                {
                    isStaticEventGeneric = true;
                    genericEventType = baseType;

                    // Get the Raise method that takes (object sender, T args)
                    Type[] genericArgs = baseType.GetGenericArguments();
                    raiseMethod = baseType.GetMethod("Raise", new Type[] { typeof(object), genericArgs[0] });

                    // Get the testValue field
                    testValueField = baseType.GetField("testValue", BindingFlags.NonPublic | BindingFlags.Instance);

                    break;
                }
                baseType = baseType.BaseType;
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Only add test controls for StaticEvent<T> derived classes
            if (isStaticEventGeneric)
            {
                // Add some space
                EditorGUILayout.Space();

                // Add a section header
                EditorGUILayout.LabelField("Test Event", EditorStyles.boldLabel);

                // Create the test button
                if (GUILayout.Button("Raise Event with Test Value"))
                {
                    // Get the test value
                    object testValue = testValueField?.GetValue(target);

                    // Call Raise(null, testValue) using reflection
                    raiseMethod?.Invoke(target, new object[] { null, testValue });

                    // Log the action
                    Type argType = genericEventType.GetGenericArguments()[0];
                    string testValueStr = testValue?.ToString() ?? "null";
                    Debug.Log($"StaticEvent<{argType.Name}> '{target.name}' raised with test value: {testValueStr}");
                }

                // Add some help text
                string argTypeName = genericEventType?.GetGenericArguments()[0].Name ?? "Unknown";
                EditorGUILayout.HelpBox($"Click the button above to test the StaticEvent<{argTypeName}> by calling Raise(null, testValue). The testValue can be set in the field above. Check the console for confirmation.", MessageType.Info);
            }
        }
    }
}