/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using JFramework.ObjectPooling;
using System.Reflection;
using System.Collections;

namespace JFramework.Editor
{
    /// <summary>
    /// PhysicsPoolable 組件的自定義 Inspector
    /// 繼承 PoolableEditorBase 並擴展物理回收功能
    /// </summary>
    [CustomEditor(typeof(PhysicsPoolable))]
    public class PhysicsPoolableEditor : PoolableEditorBase
    {
        // PhysicsPoolable 特有字段
        private FieldInfo sleepRecycleCoroutineField;
        private FieldInfo enableSleepTimerRecycleField;
        private FieldInfo sleepRecycleTimeField;
        private FieldInfo enableCollisionRecycleField;
        private FieldInfo enableTriggerRecycleField;
        private FieldInfo collisionRecycleLayersField;
        private FieldInfo triggerRecycleLayersField;
        private FieldInfo cachedRigidbodyField;
        private FieldInfo cachedRigidbody2DField;

        protected override void OnEnable()
        {
            // 調用基礎類的 OnEnable
            base.OnEnable();

            // 獲取 PhysicsPoolable 特有字段
            var physicsPoolableType = typeof(PhysicsPoolable);
            sleepRecycleCoroutineField = physicsPoolableType.GetField("sleepRecycleCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
            enableSleepTimerRecycleField = physicsPoolableType.GetField("enableSleepTimerRecycle", BindingFlags.NonPublic | BindingFlags.Instance);
            sleepRecycleTimeField = physicsPoolableType.GetField("sleepRecycleTime", BindingFlags.NonPublic | BindingFlags.Instance);
            enableCollisionRecycleField = physicsPoolableType.GetField("enableCollisionRecycle", BindingFlags.NonPublic | BindingFlags.Instance);
            enableTriggerRecycleField = physicsPoolableType.GetField("enableTriggerRecycle", BindingFlags.NonPublic | BindingFlags.Instance);
            collisionRecycleLayersField = physicsPoolableType.GetField("collisionRecycleLayers", BindingFlags.NonPublic | BindingFlags.Instance);
            triggerRecycleLayersField = physicsPoolableType.GetField("triggerRecycleLayers", BindingFlags.NonPublic | BindingFlags.Instance);
            cachedRigidbodyField = physicsPoolableType.GetField("cachedRigidbody", BindingFlags.NonPublic | BindingFlags.Instance);
            cachedRigidbody2DField = physicsPoolableType.GetField("cachedRigidbody2D", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override string GetRuntimeStatusTitle()
        {
            return "運行時物理回收狀態";
        }

        protected override void DrawRuntimeStatusContent(Poolable poolable)
        {
            var physicsPoolable = (PhysicsPoolable)poolable;

            // 獲取物理組件
            var cachedRigidbody = GetPrivateFieldValue<Rigidbody>(physicsPoolable, cachedRigidbodyField);
            var cachedRigidbody2D = GetPrivateFieldValue<Rigidbody2D>(physicsPoolable, cachedRigidbody2DField);

            // 顯示物理狀態
            DrawPhysicsStatus(cachedRigidbody, cachedRigidbody2D);

            EditorGUILayout.Space(5);

            // 顯示基礎定時回收狀態（重用基礎類的方法）
            DrawBasicRecycleStatus(poolable);

            EditorGUILayout.Space(5);

            // 顯示物理休眠回收狀態
            DrawSleepRecycleStatus(physicsPoolable, cachedRigidbody, cachedRigidbody2D);

            EditorGUILayout.Space(5);

            // 顯示碰撞和觸發器回收狀態
            DrawCollisionTriggerRecycleStatus(physicsPoolable);
        }

        private void DrawPhysicsStatus(Rigidbody rb, Rigidbody2D rb2D)
        {
            EditorGUILayout.LabelField("物理組件狀態", EditorStyles.boldLabel);

            bool hasPhysics = rb != null || rb2D != null;
            if (!hasPhysics)
            {
                EditorGUILayout.HelpBox("未檢測到 Rigidbody 組件", MessageType.Warning);
                return;
            }

            bool isCurrentlySleeping = false;
            if (rb != null)
                isCurrentlySleeping = rb.IsSleeping();
            else if (rb2D != null)
                isCurrentlySleeping = rb2D.IsSleeping();

            // 顯示物理狀態
            var physicsStatus = isCurrentlySleeping ? "休眠中" : "活動中";
            var statusColor = isCurrentlySleeping ? Color.cyan : Color.yellow;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("物理狀態:", GUILayout.Width(70));
            var oldColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(physicsStatus, EditorStyles.boldLabel);
            GUI.color = oldColor;
            EditorGUILayout.EndHorizontal();

            // 顯示組件類型
            string componentType = rb != null ? "Rigidbody" : "Rigidbody2D";
            EditorGUILayout.LabelField($"組件類型: {componentType}");
        }

        private void DrawSleepRecycleStatus(PhysicsPoolable physicsPoolable, Rigidbody rb, Rigidbody2D rb2D)
        {
            EditorGUILayout.LabelField("物理休眠回收", EditorStyles.boldLabel);

            // 獲取物理回收設置
            var enableSleepTimerRecycle = GetPrivateFieldValue<bool>(physicsPoolable, enableSleepTimerRecycleField);
            var sleepRecycleTime = GetPrivateFieldValue<float>(physicsPoolable, sleepRecycleTimeField);
            var sleepRecycleCoroutine = GetPrivateFieldValue<Coroutine>(physicsPoolable, sleepRecycleCoroutineField);

            if (!enableSleepTimerRecycle)
            {
                EditorGUILayout.LabelField("未啟用休眠回收", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (rb == null && rb2D == null)
            {
                EditorGUILayout.HelpBox("需要 Rigidbody 組件才能使用休眠回收功能", MessageType.Warning);
                return;
            }

            if (sleepRecycleTime <= 0)
            {
                EditorGUILayout.LabelField("休眠回收時間設為 0，功能已停用", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUILayout.LabelField($"休眠回收時間: {sleepRecycleTime:F1} 秒");

            bool isCurrentlySleeping = false;
            if (rb != null)
                isCurrentlySleeping = rb.IsSleeping();
            else if (rb2D != null)
                isCurrentlySleeping = rb2D.IsSleeping();

            if (sleepRecycleCoroutine != null)
            {
                EditorGUILayout.LabelField("狀態: 休眠監控中", EditorStyles.miniLabel);
                if (isCurrentlySleeping)
                {
                    EditorGUILayout.HelpBox("物件已休眠，正在倒數回收...", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("等待物件進入休眠狀態", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.LabelField("狀態: 已停止", EditorStyles.miniLabel);
            }
        }

        private void DrawCollisionTriggerRecycleStatus(PhysicsPoolable physicsPoolable)
        {
            EditorGUILayout.LabelField("碰撞與觸發器回收", EditorStyles.boldLabel);

            // 獲取碰撞和觸發器回收設置
            var enableCollisionRecycle = GetPrivateFieldValue<bool>(physicsPoolable, enableCollisionRecycleField);
            var enableTriggerRecycle = GetPrivateFieldValue<bool>(physicsPoolable, enableTriggerRecycleField);
            var collisionRecycleLayers = GetPrivateFieldValue<LayerMask>(physicsPoolable, collisionRecycleLayersField);
            var triggerRecycleLayers = GetPrivateFieldValue<LayerMask>(physicsPoolable, triggerRecycleLayersField);

            if (!enableCollisionRecycle && !enableTriggerRecycle)
            {
                EditorGUILayout.LabelField("未啟用碰撞或觸發器回收", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // 顯示碰撞回收狀態
            if (enableCollisionRecycle)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("✓ 碰撞回收已啟用", EditorStyles.boldLabel);
                DrawLayerMaskInfo("碰撞 Layers", collisionRecycleLayers);
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("✗ 碰撞回收未啟用", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space(3);

            // 顯示觸發器回收狀態
            if (enableTriggerRecycle)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("✓ 觸發器回收已啟用", EditorStyles.boldLabel);
                DrawLayerMaskInfo("觸發器 Layers", triggerRecycleLayers);
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("✗ 觸發器回收未啟用", EditorStyles.centeredGreyMiniLabel);
            }

            // 顯示啟用的功能提示
            if (enableCollisionRecycle || enableTriggerRecycle)
            {
                EditorGUILayout.Space(3);
                string activeFeatures = "";
                if (enableCollisionRecycle && enableTriggerRecycle)
                    activeFeatures = "物件將在指定 Layer 的物件碰撞或觸發時回收";
                else if (enableCollisionRecycle)
                    activeFeatures = "物件將在指定 Layer 的物件碰撞時回收";
                else if (enableTriggerRecycle)
                    activeFeatures = "物件將在指定 Layer 的物件觸發時回收";

                EditorGUILayout.HelpBox(activeFeatures, MessageType.Info);
            }
        }

        private void DrawLayerMaskInfo(string label, LayerMask layerMask)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{label}:", GUILayout.Width(80));
            
            if (layerMask.value == -1)
            {
                EditorGUILayout.LabelField("所有 Layers", EditorStyles.miniLabel);
            }
            else if (layerMask.value == 0)
            {
                EditorGUILayout.LabelField("無", EditorStyles.miniLabel);
            }
            else
            {
                // 顯示選中的 Layer 名稱
                var layerNames = GetLayerNames(layerMask);
                if (layerNames.Count <= 3)
                {
                    EditorGUILayout.LabelField(string.Join(", ", layerNames), EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"{layerNames.Count} 個 Layers", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private System.Collections.Generic.List<string> GetLayerNames(LayerMask layerMask)
        {
            var layerNames = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 32; i++)
            {
                if ((layerMask.value & (1 << i)) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        layerNames.Add(layerName);
                    }
                    else
                    {
                        layerNames.Add($"Layer {i}");
                    }
                }
            }
            return layerNames;
        }
    }
}