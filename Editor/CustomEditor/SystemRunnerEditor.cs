/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.SOAP.ModularSystem;

namespace JFramework.Editor
{
    /// <summary>
    /// SystemRunner 的自定義編輯器
    /// 在 Inspector 中顯示運行時訂閱狀態資訊
    /// </summary>
    [CustomEditor(typeof(SystemRunner))]
    public class SystemRunnerEditor : UnityEditor.Editor
    {
        private SystemRunner systemRunner;
        private bool showRuntimeInfo = true;
        private string lastSubscriptionStatus = "";
        private double lastUpdateTime = 0f;

        private void OnEnable()
        {
            systemRunner = (SystemRunner)target;
        }

        public override void OnInspectorGUI()
        {
            // 繪製默認 Inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // 只在 Play Mode 下顯示運行時資訊
            if (Application.isPlaying)
            {
                DrawRuntimeInformation();
            }
            else
            {
                DrawNonPlayModeInfo();
            }

            // 如果在 Play Mode 且已初始化，持續更新顯示
            if (Application.isPlaying && systemRunner != null)
            {
                // 每秒更新一次
                if (EditorApplication.timeSinceStartup - lastUpdateTime > 1.0)
                {
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    Repaint();
                }
            }
        }

        private void DrawRuntimeInformation()
        {
            EditorGUILayout.BeginVertical("box");

            showRuntimeInfo = EditorGUILayout.Foldout(showRuntimeInfo, "Runtime Information", true);

            if (showRuntimeInfo)
            {
                EditorGUILayout.Space(5);

                var modules = systemRunner.GetAllModules();

                EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
                if (modules != null)
                {
                    foreach (var module in modules)
                    {
                        EditorGUILayout.LabelField("- " + (module != null ? module.ToString() : "Null"));
                    }
                }

                // 獲取並顯示訂閱狀態
                string currentStatus = systemRunner.GetSubscriptionStatus();

                if (!string.IsNullOrEmpty(currentStatus))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Subscription Status:", EditorStyles.boldLabel);

                    // 使用 HelpBox 樣式顯示狀態資訊
                    EditorGUILayout.HelpBox(currentStatus, MessageType.Info);

                    lastSubscriptionStatus = currentStatus;
                }

                EditorGUILayout.Space(5);

                // 手動刷新按鈕
                if (GUILayout.Button("Refresh Status"))
                {
                    Repaint();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNonPlayModeInfo()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime Information", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Runtime subscription information is only available during Play Mode.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
    }
}