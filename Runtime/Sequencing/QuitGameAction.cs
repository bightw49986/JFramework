/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JFramework.Sequencing
{
    public class QuitGameAction : SequenceAction
    {
        public override void OnActionStart()
        {
            if (Application.isEditor)
            {
                Debug.LogWarning("QuitGameAction called in Editor. Stopping Play Mode.");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                Application.Quit();
            }
        }

        public override IEnumerator SequenceRoutine()
        {
            yield break;
        }
    }
}