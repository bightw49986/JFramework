/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Collections;
using JFramework.SceneManagement;
using UnityEngine;


namespace JFramework.Sequencing
{
    public class ChangeSceneAction : SequenceAction
    {
        [SerializeField]
        private ChangeSceneInfo changeSceneInfo;

        [SerializeField]
        private ChangeSceneEvent changeSceneEvent;

        public override IEnumerator SequenceRoutine()
        {
            changeSceneEvent.Raise(this, changeSceneInfo);
            yield break;
        }
    }
}