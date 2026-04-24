/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.ObjectPooling;
using System.Reflection;

namespace JFramework.Editor
{
    /// <summary>
    /// Poolable 組件編輯器的基礎類，提供共用的編輯器功能
    /// </summary>
    [CustomEditor(typeof(Poolable))]
    public class PoolableEditorBase : UnityEditor.Editor
    {
        protected GUIStyle statusBoxStyle;
        protected bool showRuntimeStatus = true;

        // 用於獲取私有字段的反射信息
        protected FieldInfo autoRecycleCoroutineField;
        protected FieldInfo enableAutoRecycleField;
        protected FieldInfo autoRecycleTimeField;

        protected virtual void OnEnable()
        {
            // 獲取 Poolable 基礎字段的反射信息
            var poolableType = typeof(Poolable);
            autoRecycleCoroutineField = poolableType.GetField("autoRecycleCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
            enableAutoRecycleField = poolableType.GetField("enableAutoRecycle", BindingFlags.NonPublic | BindingFlags.Instance);
            autoRecycleTimeField = poolableType.GetField("autoRecycleTime", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            // 繪製默認 Inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            // 初始化樣式
            InitializeStyles();
            
            var poolable = (Poolable)target;
            
            // 只在播放模式下顯示運行時狀態
            if (Application.isPlaying)
            {
                DrawRuntimeStatusSection(poolable);
            }
        }

        protected void InitializeStyles()
        {
            if (statusBoxStyle == null)
            {
                statusBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }
        }

        protected virtual void DrawRuntimeStatusSection(Poolable poolable)
        {
            // 可折疊的運行時狀態區域
            showRuntimeStatus = EditorGUILayout.Foldout(showRuntimeStatus, GetRuntimeStatusTitle(), true, EditorStyles.foldoutHeader);
            
            if (!showRuntimeStatus) return;

            EditorGUILayout.BeginVertical(statusBoxStyle);
            
            if (!poolable.IsActive)
            {
                EditorGUILayout.LabelField("物件已回收", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawRuntimeStatusContent(poolable);
            }
            
            EditorGUILayout.EndVertical();
        }

        protected virtual string GetRuntimeStatusTitle()
        {
            return "運行時自動回收狀態";
        }

        protected virtual void DrawRuntimeStatusContent(Poolable poolable)
        {
            DrawBasicRecycleStatus(poolable);
        }

        protected void DrawBasicRecycleStatus(Poolable poolable)
        {
            EditorGUILayout.LabelField("基礎定時回收", EditorStyles.boldLabel);

            // 獲取基礎自動回收設置
            var enableAutoRecycle = GetPrivateFieldValue<bool>(poolable, enableAutoRecycleField);
            var autoRecycleTime = GetPrivateFieldValue<float>(poolable, autoRecycleTimeField);
            var autoRecycleCoroutine = GetPrivateFieldValue<Coroutine>(poolable, autoRecycleCoroutineField);

            if (enableAutoRecycle && autoRecycleTime > 0)
            {
                EditorGUILayout.LabelField($"回收時間: {autoRecycleTime:F1} 秒");

                if (autoRecycleCoroutine != null)
                {
                    EditorGUILayout.LabelField("狀態: 計時回收中", EditorStyles.miniLabel);
                    EditorGUILayout.HelpBox("定時自動回收正在運行中", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField("狀態: 已停止", EditorStyles.miniLabel);
                }
            }
            else if (!enableAutoRecycle)
            {
                EditorGUILayout.LabelField("未啟用定時回收", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("定時回收時間設為 0，功能已停用", EditorStyles.centeredGreyMiniLabel);
            }
        }

        protected T GetPrivateFieldValue<T>(object obj, FieldInfo field)
        {
            if (field == null) return default(T);
            return (T)field.GetValue(obj);
        }
    }

    /// <summary>
    /// Poolable 組件的自定義 Inspector
    /// 顯示默認 Inspector 以及運行時的自動回收狀態
    /// </summary>
    [CustomEditor(typeof(Poolable))]
    public class PoolableEditor : PoolableEditorBase
    {
        protected override void DrawRuntimeStatusContent(Poolable poolable)
        {
            // 獲取基礎自動回收設置
            var enableAutoRecycle = GetPrivateFieldValue<bool>(poolable, enableAutoRecycleField);
            var autoRecycleTime = GetPrivateFieldValue<float>(poolable, autoRecycleTimeField);
            var autoRecycleCoroutine = GetPrivateFieldValue<Coroutine>(poolable, autoRecycleCoroutineField);
            
            if (enableAutoRecycle && autoRecycleTime > 0)
            {
                EditorGUILayout.LabelField("自動回收設置", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"回收時間: {autoRecycleTime:F1} 秒");
                
                if (autoRecycleCoroutine != null)
                {
                    EditorGUILayout.LabelField("狀態: 計時回收中", EditorStyles.miniLabel);
                    EditorGUILayout.HelpBox("定時自動回收正在運行中", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField("狀態: 已停止", EditorStyles.miniLabel);
                }
            }
            else if (!enableAutoRecycle)
            {
                EditorGUILayout.LabelField("未啟用自動回收", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("自動回收時間設為 0，功能已停用", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }
}
