/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;
using JFramework.Sequencing;

namespace JFramework.Editor
{
    [CustomEditor(typeof(Sequencer), true)]
    public class SequencerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Sequencer sequencer = (Sequencer)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sequence Controls", EditorStyles.boldLabel);

            if (GUILayout.Button("Force Next Action"))
            {
                sequencer.ForceNextAction();
            }
            if (GUILayout.Button("Force Complete Sequence"))
            {
                sequencer.ForceCompleteSequence();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh All Actions"))
            {
                sequencer.RefreshAllActions();
            }
        }
    }
}
