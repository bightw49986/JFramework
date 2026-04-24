/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Threading.Tasks;
using JFramework.SOAP;

namespace JFramework.SceneManagement
{
    public interface ISceneTransition
    {
        Task Perform();
    }

    public abstract class SceneTransitionBase : ScriptableTask, ISceneTransition
    {
        public abstract Task Perform();

        public sealed override Task Execute()
        {
            return Perform();
        }
    }
}