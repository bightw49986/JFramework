/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.ObjectPooling;

namespace JFramework.Audio
{
    /// <summary>
    /// AudioSource播放器包裝類，用於池化管理
    /// </summary>
    public class AudioSourcePlayer : MonoBehaviour, IPoolableObject<GameObject>
    {
        public AudioSource AudioSource { get; private set; }
        public string AudioID { get; private set; }
        public string GroupID { get; private set; }
        public float OriginalVolume { get; private set; }
        public int Priority { get; set; } = 128; // Unity AudioSource默認優先權
        
        // IPoolableObject<GameObject> 實現
        public GameObject Prototype { get; set; }
        public bool IsActive => gameObject.activeSelf;
        public string Name 
        { 
            get => gameObject.name;
            set => gameObject.name = value;
        }
        
        public event System.Action<IPoolableObject> RecycleRequested;
        
        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
            if (AudioSource == null)
                AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.spatialBlend = 0f; // 預設為2D音效
        }
        
        public void Setup(AudioClip clip, string audioID, string groupID, float volume, float pitch, bool loop, bool is3D = false, Vector3? position = null, AudioGroupSettings? groupSettings = null)
        {
            AudioSource.clip = clip;
            AudioID = audioID;
            GroupID = groupID;
            OriginalVolume = volume;
            AudioSource.volume = volume;
            AudioSource.pitch = pitch;
            AudioSource.loop = loop;
            
            // 應用組設定的優先權
            if (groupSettings.HasValue)
            {
                Priority = groupSettings.Value.Priority;
                AudioSource.priority = Priority;
                
                // 3D音效設定
                if (groupSettings.Value.Enable3D || is3D)
                {
                    AudioSource.spatialBlend = 1f;
                    AudioSource.minDistance = groupSettings.Value.MinDistance > 0 ? groupSettings.Value.MinDistance : 1f;
                    AudioSource.maxDistance = groupSettings.Value.MaxDistance > 0 ? groupSettings.Value.MaxDistance : 500f;
                    AudioSource.rolloffMode = groupSettings.Value.RolloffMode;
                    
                    if (position.HasValue)
                    {
                        transform.position = position.Value;
                    }
                }
                else
                {
                    AudioSource.spatialBlend = 0f; // 2D音效
                }
            }
            else
            {
                // 3D音效設定（無組設定時）
                if (is3D)
                {
                    AudioSource.spatialBlend = 1f; // 完全3D
                    if (position.HasValue)
                    {
                        transform.position = position.Value;
                    }
                }
                else
                {
                    AudioSource.spatialBlend = 0f; // 2D音效
                }
            }
        }
        
        public void Setup3DAudio(Vector3 position, float minDistance = 1f, float maxDistance = 500f)
        {
            AudioSource.spatialBlend = 1f;
            AudioSource.minDistance = minDistance;
            AudioSource.maxDistance = maxDistance;
            AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            transform.position = position;
        }
        
        public void ResetAudio()
        {
            AudioSource.Stop();
            AudioSource.clip = null;
            AudioSource.volume = 1f;
            AudioSource.pitch = 1f;
            AudioSource.loop = false;
            AudioSource.spatialBlend = 0f;
            AudioSource.minDistance = 1f;
            AudioSource.maxDistance = 500f;
            AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            AudioID = null;
            GroupID = null;
            OriginalVolume = 1f;
            Priority = 128;
            transform.position = Vector3.zero;
        }

        public void RequestRecycle()
        {
            RecycleRequested?.Invoke(this);
        }
        
        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void OnRecycle()
        {
            ResetAudio();
            gameObject.SetActive(false);
        }
    }
}