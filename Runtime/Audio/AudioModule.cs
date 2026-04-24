/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Threading.Tasks;
using JFramework.ObjectPooling;
using System.Collections.Generic;
using JFramework.SOAP.ModularSystem;

namespace JFramework.Audio
{
    [CreateAssetMenu(menuName = "JFramework/System/Audio")]
    public class AudioModule : SystemModule
    {
        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        private List<AudioGroup> audioGroups = new List<AudioGroup>();

        [SerializeField]
        private PlayAudioEvent playAudioEvent;

        [SerializeField]
        private StopAudioEvent stopAudioEvent;
        
        private AudioManager audioManager;

        public override void Initialize(GameObject objectRoot = null)
        {
            base.Initialize(objectRoot);
            
            // 初始化AudioManager（不再需要coroutineRunner）
            audioManager = new AudioManager(objectRoot?.transform);

            // 註冊所有音效組
            foreach (var audioGroup in audioGroups)
            {
                audioManager.RegisterAudioGroup(audioGroup);
            }

            playAudioEvent.AddListener(OnPlayAudio);
            stopAudioEvent.AddListener(OnStopAudio);
            
            Debug.Log($"[AudioModule] Initialized with {audioGroups.Count} audio groups");
        }

        private void OnStopAudio(object sender, StopAudioInfo e)
        {
            StopGroup(e.GroupID, e.FadeOutDuration);
        }

        private void OnPlayAudio(object sender, PlayAudioInfo e)
        {
            PlayAudio(e.AudioID, e.GroupID, e.Volume, e.Pitch, e.Loop, e.FadeInDuration, e.NormalizedTime);
        }

        public override void Terminate()
        {
            playAudioEvent.RemoveListener(OnPlayAudio);
            stopAudioEvent.RemoveListener(OnStopAudio);
            audioManager?.Dispose();
            base.Terminate();
        }
        
        // 對外提供的API接口
        public AudioSourcePlayer PlayAudio(string audioID, string groupID = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f, float normalizedTime = 0f)
        {
            return audioManager?.PlayAudio(audioID, groupID, volume, pitch, loop, fadeInDuration, false, null, normalizedTime);
        }

        public AudioSourcePlayer PlayAudio3D(string audioID, Vector3 position, string groupID = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f, float normalizedTime = 0f)
        {
            return audioManager?.PlayAudio(audioID, groupID, volume, pitch, loop, fadeInDuration, true, position, normalizedTime);
        }

        /// <summary>
        /// 播放音效（從PlayAudioInfo結構體）
        /// </summary>
        public AudioSourcePlayer PlayAudio(PlayAudioInfo audioInfo)
        {
            return PlayAudio(audioInfo.AudioID.Value, audioInfo.GroupID.Value, audioInfo.Volume, 
                           audioInfo.Pitch, audioInfo.Loop, audioInfo.FadeInDuration, audioInfo.NormalizedTime);
        }

        /// <summary>
        /// 播放3D音效（從PlayAudioInfo結構體）
        /// </summary>
        public AudioSourcePlayer PlayAudio3D(PlayAudioInfo audioInfo, Vector3 position)
        {
            return PlayAudio3D(audioInfo.AudioID.Value, position, audioInfo.GroupID.Value, audioInfo.Volume, 
                              audioInfo.Pitch, audioInfo.Loop, audioInfo.FadeInDuration, audioInfo.NormalizedTime);
        }

        public void StopAudio(AudioSourcePlayer player, float fadeOutDuration = 0f)
        {
            audioManager?.StopAudio(player, fadeOutDuration);
        }

        public void StopAudio(string audioID, float fadeOutDuration = 0f)
        {
            audioManager?.StopAudio(audioID, fadeOutDuration);
        }

        public void StopGroup(string groupID, float fadeOutDuration = 0f)
        {
            audioManager?.StopGroup(groupID, fadeOutDuration);
        }

        /// <summary>
        /// 使用ObjectPooling框架預載入音效池
        /// </summary>
        public async Task PreloadAudioClipsAsync(List<WarmupConfig<GameObject>> warmupConfigs)
        {
            if (audioManager != null)
                await audioManager.PreloadAudioClipsAsync(warmupConfigs);
        }
        
        /// <summary>
        /// 便利方法：為特定音效ID預載入池
        /// </summary>
        public async Task PreloadAudioClipsAsync(List<string> audioIDs = null, int countPerAudio = 5, int splitIntoFrames = 2)
        {
            if (audioManager != null)
                await audioManager.PreloadAudioClipsAsync(audioIDs, countPerAudio, splitIntoFrames);
        }
    }
}