/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using IUpdateSubscription = JFramework.SystemRunner.IUpdateSubscription;

namespace JFramework.StateMachine
{
    /// <summary>
    /// 支援模組化 UpdateManager 的 InitiativeStateMachine 核心功能
    /// </summary>
    public sealed class InitiativeStateMachineCore : IDisposable
    {
        private IUpdateSubscription updateSubscription;
        private bool disposedValue;
        private bool isRunning;
        private readonly InitiativeSMConfig config;
        private readonly IStateMachine stateMachine;
        private readonly SystemRunner systemRunner;

        public InitiativeStateMachineCore(IStateMachine stateMachine, InitiativeSMConfig config, SystemRunner systemRunner)
        {
            this.stateMachine = stateMachine;
            this.config = config;
            this.systemRunner = systemRunner;

            if (config.initialStateType != null)
            {
                try
                {
                    if (stateMachine.SetNextState(config.initialStateType, config.initialStateChangeArgument, deferToNextStateUpdate: true))
                        Run();
                }
                catch (StateMachineException e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Run()
        {
            if (isRunning)
                return;

            StartRunning(stateMachine, config);
            isRunning = true;
        }

        public void Stop(bool terminateCurrentState = true)
        {
            if (!isRunning)
                return;

            updateSubscription?.Dispose();
            updateSubscription = null;

            if (terminateCurrentState)
            {
                stateMachine.SetNextState(null);
            }

            isRunning = false;
        }

        private void StartRunning(IStateMachine stateMachine, InitiativeSMConfig config)
        {
            if (systemRunner == null)
            {
                Debug.LogError("[InitiativeStateMachineCore] UpdateManagerModule is null! Cannot start state machine.");
                return;
            }

            Action updateAction = () => stateMachine.UpdateCurrentState(config.updateArgumentGetter?.Invoke());

            switch (config.updateTiming)
            {
                case UpdateTiming.Update:
                    updateSubscription = systemRunner.SubscribeToUpdate(updateAction);
                    break;
                case UpdateTiming.FixedUpdate:
                    updateSubscription = systemRunner.SubscribeToFixedUpdate(updateAction);
                    break;
                case UpdateTiming.LateUpdate:
                    updateSubscription = systemRunner.SubscribeToLateUpdate(updateAction);
                    break;
                case UpdateTiming.CustomInterval:
                    updateSubscription = systemRunner.SubscribeToCustomInterval(updateAction, config.customUpdateInterval);
                    break;
                default:
                    Debug.LogWarning($"[InitiativeStateMachineCore] Unsupported update timing: {config.updateTiming}");
                    break;
            }

            if (updateSubscription == null)
            {
                Debug.LogError("[InitiativeStateMachineCore] Failed to create update subscription!");
            }
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    updateSubscription?.Dispose();
                    updateSubscription = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}