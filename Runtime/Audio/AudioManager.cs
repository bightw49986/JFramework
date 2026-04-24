/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using JFramework.ObjectPooling;

namespace JFramework.Audio
{
    public class AudioManager
    {
        // 音效資源映射表
        private readonly Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        // 音效組設定映射表
        private readonly Dictionary<string, AudioGroupSettings> groupSettings = new Dictionary<string, AudioGroupSettings>();

        // 正在播放的音效追蹤（按AudioID分組）
        private readonly Dictionary<string, List<AudioSourcePlayer>> playingAudio = new Dictionary<string, List<AudioSourcePlayer>>();

        // 所有活躍的音效播放器追蹤
        private readonly List<AudioSourcePlayer> allActivePlayers = new List<AudioSourcePlayer>();

        // 使用JFramework的ObjectPooling系統
        private AudioSourcePlayerPoolManager poolManager;
        private GameObject audioSourcePrefab;
        private Transform audioSourceContainer;

        // 淡入淡出追蹤
        private readonly HashSet<AudioSourcePlayer> fadingPlayers = new HashSet<AudioSourcePlayer>();

        public AudioManager(Transform parentContainer = null)
        {
            // 創建音效容器
            var containerObject = new GameObject("AudioSourceContainer");
            audioSourceContainer = containerObject.transform;
            if (parentContainer != null)
                audioSourceContainer.SetParent(parentContainer);

            // 創建AudioSource prefab模板
            CreateAudioSourcePrefab();

            // 初始化Pool管理器
            poolManager = new AudioSourcePlayerPoolManager(10, audioSourceContainer);

            /// <summary>
            /// 創建AudioSource prefab模板
            /// </summary>
            void CreateAudioSourcePrefab()
            {
                var prefabObject = new GameObject("AudioSourcePrefab");
                prefabObject.AddComponent<AudioSource>();
                prefabObject.AddComponent<AudioSourcePlayer>();
                prefabObject.SetActive(false); // prefab預設為非啟用狀態

                audioSourcePrefab = prefabObject;
            }
        }

        /// <summary>
        /// 註冊音效組
        /// </summary>
        public void RegisterAudioGroup(AudioGroup audioGroup)
        {
            if (audioGroup == null) return;

            var groupID = audioGroup.Settings.GroupID.Value;
            groupSettings[groupID] = audioGroup.Settings;

            // 註冊該組所有音效
            foreach (var audioSet in audioGroup.AudioSets)
            {
                if (audioSet.AudioClip != null)
                {
                    audioClips[audioSet.AudioID.Value] = audioSet.AudioClip;
                }
            }

            // 初始化該組的播放列表
            if (!playingAudio.ContainsKey(groupID))
            {
                playingAudio[groupID] = new List<AudioSourcePlayer>();
            }
        }

        /// <summary>
        /// 使用ObjectPooling框架進行時間分割預載入
        /// </summary>
        public async Task PreloadAudioClipsAsync(List<WarmupConfig<GameObject>> warmupConfigs)
        {
            if (poolManager != null && warmupConfigs != null && warmupConfigs.Count > 0)
            {
                await poolManager.ExecuteTimeSlicedWarmupAsync(warmupConfigs);
                Debug.Log($"[AudioManager] Preloaded {warmupConfigs.Count} audio pool configurations using ObjectPooling framework");
            }
        }

        /// <summary>
        /// 為特定音效ID建立預載入配置的便利方法
        /// </summary>
        public async Task PreloadAudioClipsAsync(List<string> audioIDs = null, int countPerAudio = 5, int splitIntoFrames = 2)
        {
            if (audioIDs == null || audioIDs.Count == 0)
                return;

            var warmupConfigs = new List<WarmupConfig<GameObject>>();
            
            foreach (var audioID in audioIDs)
            {
                if (audioClips.ContainsKey(audioID))
                {
                    // 為每個音效創建預載入配置
                    var config = new WarmupConfig<GameObject>(
                        audioSourcePrefab, 
                        countPerAudio, 
                        splitIntoFrames
                    );
                    warmupConfigs.Add(config);
                }
            }

            await PreloadAudioClipsAsync(warmupConfigs);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public AudioSourcePlayer PlayAudio(string audioID, string groupID = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f)
        {
            return PlayAudio(audioID, groupID, volume, pitch, loop, fadeInDuration, false, null, 0f);
        }

        /// <summary>
        /// 播放音效（支援3D音效）
        /// </summary>
        public AudioSourcePlayer PlayAudio(string audioID, string groupID = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f, bool is3D = false, Vector3? position = null)
        {
            return PlayAudio(audioID, groupID, volume, pitch, loop, fadeInDuration, is3D, position, 0f);
        }

        /// <summary>
        /// 播放音效（完整版本，支援3D音效和指定開始時間）
        /// </summary>
        /// <param name="audioID">音效ID</param>
        /// <param name="groupID">音效組ID</param>
        /// <param name="volume">音量 (0-1)</param>
        /// <param name="pitch">音調</param>
        /// <param name="loop">是否循環播放</param>
        /// <param name="fadeInDuration">淡入時間</param>
        /// <param name="is3D">是否使用3D音效</param>
        /// <param name="position">3D音效位置</param>
        /// <param name="normalizedTime">標準化開始時間 (0=開始，0.5=中間，1=結尾)</param>
        public AudioSourcePlayer PlayAudio(string audioID, string groupID = null, float volume = 1f, float pitch = 1f, bool loop = false, float fadeInDuration = 0f, bool is3D = false, Vector3? position = null, float normalizedTime = 0f)
        {
            if (!audioClips.ContainsKey(audioID))
            {
                Debug.LogWarning($"[AudioManager] Audio clip '{audioID}' not found!");
                return null;
            }

            var clip = audioClips[audioID];
            AudioGroupSettings? settings = null;

            // 如果指定了分組，使用分組設定
            if (!string.IsNullOrEmpty(groupID) && groupSettings.ContainsKey(groupID))
            {
                settings = groupSettings[groupID];

                // 檢查最大同時播放數量限制
                if (playingAudio[groupID].Count >= settings.Value.MaxSimultaneousPlays)
                {
                    HandleGroupPlayLimitExceeded(groupID, settings.Value);
                }

                // 應用組音量限制
                volume = Mathf.Min(volume, settings.Value.MaxVolume);

                // 處理切換行為
                HandleAudioSwitchBehavior(groupID, settings.Value, fadeInDuration);
            }

            var player = GetAudioSourcePlayer();
            player.Setup(clip, audioID, groupID, volume, pitch, loop, is3D, position, settings);

            // 確保 normalizedTime 在有效範圍內
            normalizedTime = Mathf.Clamp01(normalizedTime);

            // 加入追蹤列表
            if (!string.IsNullOrEmpty(groupID))
            {
                playingAudio[groupID].Add(player);

                // 應用平方根衰減音量管理（如果組設定啟用）
                if (settings.HasValue && settings.Value.EnableVolumeDecay)
                {
                    ApplySqrtVolumeDecay(groupID);
                }
            }

            // 播放音效
            if (fadeInDuration > 0f)
            {
                player.AudioSource.volume = 0f;
                player.AudioSource.Play();
                
                // 設定開始播放時間
                if (normalizedTime > 0f)
                {
                    player.AudioSource.time = normalizedTime * clip.length;
                }
                
                FadeIn(player, volume, fadeInDuration);
            }
            else
            {
                player.AudioSource.Play();
                
                // 設定開始播放時間
                if (normalizedTime > 0f)
                {
                    player.AudioSource.time = normalizedTime * clip.length;
                }
            }

            // 註冊播放完成回調
            _ = WaitForAudioEndAsync(player);

            return player;

            /// <summary>
            /// 從池中獲取AudioSourcePlayer
            /// </summary>
            AudioSourcePlayer GetAudioSourcePlayer()
            {
                var player = poolManager.Spawn(audioSourcePrefab);
                allActivePlayers.Add(player);
                return player;
            }

            async Task WaitForAudioEndAsync(AudioSourcePlayer player)
            {
                while (player.AudioSource != null && player.AudioSource.isPlaying)
                {
                    await Task.Yield();
                }

                OnAudioEnd(player);
            }
        }

        /// <summary>
        /// 應用平方根衰減音量管理
        /// 當同一組有多個音效同時播放時，使用平方根衰減來避免音量過載
        /// </summary>
        private void ApplySqrtVolumeDecay(string groupID)
        {
            if (!playingAudio.ContainsKey(groupID)) 
                return;

            var playingList = playingAudio[groupID];
            int simultaneousCount = playingList.Count;

            if (simultaneousCount <= 1) return;

            // 使用平方根衰減公式：adjustedVolume = originalVolume * sqrt(1 / simultaneousCount)
            float volumeMultiplier = Mathf.Sqrt(1f / simultaneousCount);

            foreach (var player in playingList)
            {
                if (player != null && player.AudioSource != null)
                {
                    // 根據原始音量計算新的音量
                    float adjustedVolume = player.OriginalVolume * volumeMultiplier;
                    player.AudioSource.volume = adjustedVolume;
                }
            }
        }

        /// <summary>
        /// 清理資源
        /// </summary>
        public void Dispose()
        {
            // 停止所有音效
            foreach (var player in allActivePlayers.ToList())
            {
                StopAudio(player);
            }

            fadingPlayers.Clear();
            audioClips.Clear();
            groupSettings.Clear();
            playingAudio.Clear();
            allActivePlayers.Clear();

            // 清理Pool管理器
            poolManager?.Dispose();

            // 銷毀音效容器和prefab
            if (audioSourceContainer != null)
            {
                Object.DestroyImmediate(audioSourceContainer.gameObject);
            }

            if (audioSourcePrefab != null)
            {
                Object.DestroyImmediate(audioSourcePrefab);
            }
        }

        // 私有輔助方法
        private void HandleGroupPlayLimitExceeded(string groupID, AudioGroupSettings settings)
        {
            var playingList = playingAudio[groupID];
            if (playingList.Count > 0)
            {
                // 停止最舊的音效
                var oldestPlayer = playingList[0];
                StopAudio(oldestPlayer);
            }
        }

        private void HandleAudioSwitchBehavior(string groupID, AudioGroupSettings settings, float fadeInDuration)
        {
            var playingList = playingAudio[groupID];

            switch (settings.SwitchBehavior)
            {
                case AudioSwitchBehavior.InstantCut:
                    // 立即停止所有同組音效
                    foreach (var player in playingList.ToList())
                    {
                        StopAudio(player, 0f);
                    }
                    break;

                case AudioSwitchBehavior.FadeInOut:
                    // 淡出所有同組音效
                    float fadeDuration = fadeInDuration > 0f ? fadeInDuration : 0.5f;
                    foreach (var player in playingList.ToList())
                    {
                        StopAudio(player, fadeDuration);
                    }
                    break;

                case AudioSwitchBehavior.Overlap:
                    // 允許重疊播放，不做處理
                    break;
            }
        }

        /// <summary>
        /// 停止音效播放
        /// </summary>
        public void StopAudio(AudioSourcePlayer player, float fadeOutDuration = 0f)
        {
            if (player == null || !allActivePlayers.Contains(player))
                return;

            if (fadeOutDuration > 0f)
            {
                _ = FadeOutAsync(player, fadeOutDuration);
            }
            else
            {
                player.AudioSource.Stop();
                OnAudioEnd(player);
            }
        }

        /// <summary>
        /// 停止指定音效ID的所有播放
        /// </summary>
        public void StopAudio(string audioID, float fadeOutDuration = 0f)
        {
            var playersToStop = allActivePlayers.Where(p => p.AudioID == audioID).ToList();
            foreach (var player in playersToStop)
            {
                StopAudio(player, fadeOutDuration);
            }
        }

        /// <summary>
        /// 停止指定組的所有音效
        /// </summary>
        public void StopGroup(string groupID, float fadeOutDuration = 0f)
        {
            if (playingAudio.ContainsKey(groupID))
            {
                var playersToStop = playingAudio[groupID].ToList();
                foreach (var player in playersToStop)
                {
                    StopAudio(player, fadeOutDuration);
                }
            }
        }

        /// <summary>
        /// 暫停音效播放
        /// </summary>
        public void PauseAudio(AudioSourcePlayer player)
        {
            if (player != null && player.AudioSource.isPlaying)
            {
                player.AudioSource.Pause();
            }
        }

        /// <summary>
        /// 恢復音效播放
        /// </summary>
        public void ResumeAudio(AudioSourcePlayer player)
        {
            if (player != null && !player.AudioSource.isPlaying)
            {
                player.AudioSource.UnPause();
            }
        }

        private async void FadeIn(AudioSourcePlayer player, float targetVolume, float duration)
        {
            if (fadingPlayers.Contains(player)) 
                return; // 防止重複淡化

            fadingPlayers.Add(player);
            await FadeInAsync(player, targetVolume, duration);
            fadingPlayers.Remove(player);
        }

        private async Task FadeInAsync(AudioSourcePlayer player, float targetVolume, float duration)
        {
            float startVolume = player.AudioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < duration && player.AudioSource != null)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                player.AudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                await Task.Yield();
            }

            if (player.AudioSource != null)
                player.AudioSource.volume = targetVolume;
        }

        private async Task FadeOutAsync(AudioSourcePlayer player, float duration)
        {
            if (fadingPlayers.Contains(player))
                return; // 防止重複淡化

            fadingPlayers.Add(player);

            float startVolume = player.AudioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < duration && player.AudioSource != null && fadingPlayers.Contains(player))
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                player.AudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                await Task.Yield();
            }

            if (player.AudioSource != null && fadingPlayers.Contains(player))
            {
                player.AudioSource.volume = 0f;
                player.AudioSource.Stop();
            }

            fadingPlayers.Remove(player);
            OnAudioEnd(player);
        }

        /// <summary>
        /// 音效播放結束處理
        /// </summary>
        private void OnAudioEnd(AudioSourcePlayer player)
        {
            string groupID = player.GroupID;

            // 從追蹤列表中移除
            if (!string.IsNullOrEmpty(groupID) && playingAudio.ContainsKey(groupID))
            {
                playingAudio[groupID].Remove(player);

                // 重新計算該組剩餘音效的音量（如果啟用平方根衰減）
                if (groupSettings.ContainsKey(groupID) && groupSettings[groupID].EnableVolumeDecay)
                {
                    ApplySqrtVolumeDecay(groupID);
                }
            }

            // 回收到池中
            RecycleAudioSourcePlayer(player);

            /// <summary>
            /// 回收AudioSourcePlayer到池中
            /// </summary>
            void RecycleAudioSourcePlayer(AudioSourcePlayer player)
            {
                if (player != null)
                {
                    // 從活躍列表移除
                    allActivePlayers.Remove(player);
                    // 清理淡化標記
                    fadingPlayers.Remove(player);
                    
                    if (player.IsActive)
                        // 回收到池中
                        poolManager.Recycle(player);
                }
            }
        }
    }
}