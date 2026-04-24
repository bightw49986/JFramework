/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using JFramework.SOAP;

namespace JFramework.SceneManagement
{
    [Serializable]
    public struct ChangeSceneInfo
    {
        public StringReference SceneName;
        public SceneTransitionBase TransitionExitCurrent;
        public SceneTransitionBase TransitionEnterNew;
        public FloatReference FakeLoadingDuration;
    }
}