/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using JFramework.SOAP.ModularSystem;
using JFramework.SOAP;

namespace JFramework.SceneManagement
{
    [CreateAssetMenu(menuName = "JFramework/System/SceneModule", fileName = "SceneModule")]
    public class SceneModule : SystemModule
    {
        [SerializeField]
        private StringReference emptyScene;

        [SerializeField]
        private ChangeSceneEvent changeSceneEvent;

        private bool isChangingScene;

        public override void Initialize(GameObject objectRoot)
        {
            isChangingScene = false;
            changeSceneEvent.AddListener(ChangeSceneAsync);
        }

        public override void Terminate()
        {
            changeSceneEvent.RemoveListener(ChangeSceneAsync);
        }

        private void ChangeSceneAsync(object sender, ChangeSceneInfo e)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[SceneModule] Cannot change scenes in edit mode.");
                return;
            }

            if (isChangingScene)
            {
                Debug.LogError("[SceneModule] A scene change is already in progress.");
                return;
            }

            isChangingScene = true;

            InternalChangeSceneAsync(
                e.SceneName,
                emptyScene ?? "Empty",
                e.FakeLoadingDuration,
                e.TransitionExitCurrent,
                e.TransitionEnterNew).ContinueWith(_ => isChangingScene = false);
        }

        private static async Task InternalChangeSceneAsync(
            string sceneName,
            string emptySceneName = "Empty",
            float fakeLoadingDuration = 0f,
            ISceneTransition transitionExitCurrent = null,
            ISceneTransition transitionEnterNew = null)
        {
            try
            {
                var previousActiveScene = SceneManager.GetActiveScene();
                var loadEmptySceneTask = SceneManager.LoadSceneAsync(emptySceneName, LoadSceneMode.Additive);
                loadEmptySceneTask.allowSceneActivation = false;
    
                while (loadEmptySceneTask.progress < 0.9f)
                {
                    await Task.Yield();
                }
    
                if (transitionExitCurrent != null)
                {
                    await transitionExitCurrent.Perform();
                }
    
                loadEmptySceneTask.allowSceneActivation = true;
    
                await loadEmptySceneTask;
    
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(emptySceneName));
    
                if (previousActiveScene.isLoaded)
                {
                    var unloadTask = SceneManager.UnloadSceneAsync(previousActiveScene.name);
                    await unloadTask;
                }
    
                await Resources.UnloadUnusedAssets();
    
                GC.Collect();
    
                var loadNewSceneTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                loadNewSceneTask.allowSceneActivation = false;
    
                // 記錄開始時間
                var startTime = Time.realtimeSinceStartup;
    
                while (loadNewSceneTask.progress < 0.9f)
                {
                    await Task.Yield();
                }
    
                // 計算已經過的時間
                var elapsedTime = Time.realtimeSinceStartup - startTime;
    
                // 如果需要額外等待時間來滿足最小加載時間
                if (elapsedTime < fakeLoadingDuration)
                {
                    var remainingTime = fakeLoadingDuration - elapsedTime;
                    await Task.Delay((int)(remainingTime * 1000)); // 轉換為毫秒
                }
                
                loadNewSceneTask.allowSceneActivation = true;
                await loadNewSceneTask;
    
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    
                _ = SceneManager.UnloadSceneAsync(emptySceneName);
    
                if (transitionEnterNew != null)
                {
                    await transitionEnterNew.Perform();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}