/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace JFramework.Editor
{
    /// <summary>
    /// 提供掃描和安全刪除ScriptableObject引用的工具
    /// </summary>
    public static class ScriptableObjectReferenceTools
    {
        [MenuItem("Assets/掃描引用 ScriptableObject", false, 2000)]
        public static void ScanReferences()
        {
            var obj = Selection.activeObject;
            if (!(obj is ScriptableObject))
            {
                EditorUtility.DisplayDialog("掃描引用", "僅限 ScriptableObject 使用。", "OK");
                return;
            }

            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);

            var referencingFiles = FindFilesReferencingGUID(guid);

            if (referencingFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("掃描引用", "未發現任何引用。", "OK");
            }
            else
            {
                ScriptableObjectReferenceWindow.ShowWindow(referencingFiles);
            }
        }

        [MenuItem("Assets/掃描引用 ScriptableObject", true)]
        public static bool ScanReferences_Validate()
        {
            var obj = Selection.activeObject;
            return obj is ScriptableObject;
        }

        [MenuItem("Assets/安全刪除 ScriptableObject", false, 2001)]
        public static void SafeDelete()
        {
            var obj = Selection.activeObject;
            if (!(obj is ScriptableObject))
            {
                EditorUtility.DisplayDialog("安全刪除", "僅限 ScriptableObject 使用。", "OK");
                return;
            }

            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);

            var referencingFiles = FindFilesReferencingGUID(guid);

            if (referencingFiles.Count > 0)
            {
                if (!EditorUtility.DisplayDialog("安全刪除", $"發現 {referencingFiles.Count} 個引用，是否強制刪除？", "強制刪除", "取消"))
                    return;
            }

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("安全刪除", "已刪除 ScriptableObject。", "OK");
        }

        [MenuItem("Assets/安全刪除 ScriptableObject", true)]
        public static bool SafeDelete_Validate()
        {
            var obj = Selection.activeObject;
            return obj is ScriptableObject;
        }

        private static List<string> FindFilesReferencingGUID(string guid)
        {
            var referencingFiles = new List<string>();
            string[] searchExts = { "*.prefab", "*.unity", "*.asset" };
            string assetsPath = Application.dataPath;

            foreach (var ext in searchExts)
            {
                var files = Directory.GetFiles(assetsPath, ext, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string text = File.ReadAllText(file);
                    if (text.Contains(guid))
                    {
                        string relativePath = "Assets" + file.Replace(assetsPath, "").Replace("\\", "/");
                        referencingFiles.Add(relativePath);
                    }
                }
            }
            return referencingFiles;
        }
    }

    public class ScriptableObjectReferenceWindow : EditorWindow
    {
        private List<string> referencingFiles;
        private Vector2 scrollPos;

        public static void ShowWindow(List<string> referencingFiles)
        {
            var window = GetWindow<ScriptableObjectReferenceWindow>("引用清單");
            window.referencingFiles = referencingFiles;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"發現 {referencingFiles.Count} 個引用：", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var file in referencingFiles)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(file);
                if (GUILayout.Button("選取", GUILayout.Width(60)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(file);
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}