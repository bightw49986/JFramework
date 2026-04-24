/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

using JFramework.ObjectPooling;

namespace JFramework.Sequencing
{
    public class SpawnObjectAction : SequenceAction
    {
        public enum RepeatMode
        {
            None,
            Repeat,
            Infinity
        }

        [Required]
        [SerializeField]
        private Poolable objectToSpawn;

        [Required]
        [SerializeField]
        private ObjectPoolModule objectPoolModule;

        [SerializeField]
        private Transform spawnPoint;

        [SerializeField]
        private bool setPosition;

        [SerializeField]
        private bool setRotation;

        [SerializeField]
        private RepeatMode repeatMode = RepeatMode.None;

        [SerializeField]
        [Min(0)]
        private int repeatCount = 0;

        public UnityEvent<GameObject> ObjectSpawnedEvent = new UnityEvent<GameObject>();

        private Poolable spawnedObject;
        private int spawnCountRemian = 0;


        public override void OnActionStart()
        {
            spawnCountRemian = repeatCount;
            SpawnNewObject();
        }

        public override IEnumerator SequenceRoutine()
        {
            yield return new WaitWhile(() => repeatMode == RepeatMode.Infinity || (repeatMode == RepeatMode.Repeat && spawnCountRemian < repeatCount));
        }

        public override void OnActionEnd()
        {
            UnregisterRecycleEvent();
        }

        private void SpawnNewObject()
        {
            UnregisterRecycleEvent();
            
            spawnCountRemian--;
            var targetSpawnPoint = spawnPoint != null ? spawnPoint : transform;

            Vector3? position = setPosition ? targetSpawnPoint.position : null;
            Quaternion? rotation = setRotation ? targetSpawnPoint.rotation : null;
            spawnedObject = objectPoolModule.Spawn<Poolable>(objectToSpawn, position, rotation, spawnPoint ?? null);

            ObjectSpawnedEvent?.Invoke(spawnedObject.gameObject);
            spawnedObject.OnRecycleEvent.AddListener(OnSpawnedObjectRecycled);
        }

        private void OnSpawnedObjectRecycled(GameObject arg0)
        {
            if (repeatMode == RepeatMode.Repeat && spawnCountRemian > 0)
            {
                SpawnNewObject();
            }
            else if (repeatMode == RepeatMode.Infinity)
            {
                SpawnNewObject();
            }
        }

        private void UnregisterRecycleEvent()
        {
            if (spawnedObject != null)
            {
                spawnedObject.OnRecycleEvent.RemoveListener(OnSpawnedObjectRecycled);
                spawnedObject = null;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (objectPoolModule == null)
            {
                TryAutoAssignPoolModule();

                /// <summary>
                /// 嘗試自動搜尋並分配專案中的 ObjectPoolModule
                /// </summary>
                void TryAutoAssignPoolModule()
                {

                    // 在編輯器中搜尋專案資源中的 ObjectPoolModule
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ObjectPoolModule");
                    if (guids.Length > 0)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        ObjectPoolModule assetModule = UnityEditor.AssetDatabase.LoadAssetAtPath<ObjectPoolModule>(assetPath);

                        if (assetModule != null)
                        {
                            objectPoolModule = assetModule;
                            Debug.Log($"[Poolable] {gameObject.name} 自動分配專案資源中的 ObjectPoolModule: {assetModule.name}");

                            // 如果找到多個，提醒用戶
                            if (guids.Length > 1)
                            {
                                Debug.LogWarning($"[Poolable] 找到 {guids.Length} 個 ObjectPoolModule，已分配第一個: {assetModule.name}。如需使用其他模組，請手動指定。");
                            }
                            return;
                        }
                    }

                    Debug.LogWarning($"[Poolable] {gameObject.name} 未找到可用的 ObjectPoolModule，請手動分配或創建一個。");
                }
            }
        }
#endif

    }
}
