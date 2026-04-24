/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using JFramework.SOAP.ModularSystem;

namespace JFramework.StateMachine
{
    #region Definitions
    /// <summary>
    /// Defines the behavior of an InitiativeStateMachine.
    /// </summary>
    public readonly struct InitiativeSMConfig
    {
        /// <summary>
        /// Method used to gather argument for updating states.
        /// </summary>
        public readonly Func<object> updateArgumentGetter;
        /// <summary>
        /// Timing for calling the update state method.
        /// </summary>
        public readonly UpdateTiming updateTiming;
        /// <summary>
        /// Define the interval if a custom interval <seealso cref="UpdateTiming"/> was used.
        /// </summary>
        public readonly float customUpdateInterval;
        /// <summary>
        /// Specify the first state to run, note that if you didn't assign this, you'll have to manually call the SetNextState() and Run() method on the statemachine, otherwise it won't run.
        /// </summary>
        public readonly Type initialStateType;
        /// <summary>
        /// If a intial state exist, this is the argument for it's initialization.
        /// </summary>
        public readonly object initialStateChangeArgument;

        public InitiativeSMConfig(Func<object> updateArgumentGetter = null, UpdateTiming updateTiming = UpdateTiming.Update, float customUpdateInterval = 0.2f, Type initialStateType = null, object initialStateChangeArgument = null)
        {
            this.updateArgumentGetter = updateArgumentGetter;
            this.updateTiming = updateTiming;
            this.customUpdateInterval = customUpdateInterval;
            this.initialStateType = initialStateType;
            this.initialStateChangeArgument = initialStateChangeArgument;
        }
    }
    #endregion

    /// <summary>
    /// 主動式有限狀態機實作，基於 InitiativeSMConfig 設定自主運行
    /// 使用模組化 UpdateManager 架構，需要明確注入 UpdateManagerModule 依賴
    /// </summary>
    /// <typeparam name="TState">狀態機可處理的狀態型別</typeparam>
    public class InitiativeStateMachine<TState> : SimpleStateMachine<TState> where TState : class, IState
    {
        private readonly InitiativeStateMachineCore initiativeStateMachineCore;

        public InitiativeStateMachine(InitiativeSMConfig config, SystemRunner systemRunner) : base()
        {
            if (systemRunner == null)
            {
                throw new ArgumentNullException(nameof(systemRunner), "SystemRunner cannot be null");
            }

            initiativeStateMachineCore = new InitiativeStateMachineCore(this, config, systemRunner);
        }

        protected override void OnDisposed()
        {
            Stop();
            initiativeStateMachineCore.Dispose();
        }

        public void Run()
        {
            initiativeStateMachineCore.Run();
        }

        public void Stop(bool terminateCurrentState = true)
        {
            initiativeStateMachineCore.Stop(terminateCurrentState);
        }
    }

    /// <summary>
    /// 主動式有限狀態機實作，具有自定義上下文
    /// 使用模組化 UpdateManager 架構，需要明確注入 UpdateManagerModule 依賴
    /// </summary>
    /// <typeparam name="TState">狀態機可處理的狀態型別</typeparam>
    /// <typeparam name="TContext">狀態機在所有狀態間共享的上下文物件</typeparam>
    public class InitiativeStateMachine<TState, TContext> : SimpleStateMachine<TState, TContext> where TState : class, IState<TContext>
    {
        private readonly InitiativeStateMachineCore initiativeStateMachineCore;

        public InitiativeStateMachine(InitiativeSMConfig config, TContext machineContext, SystemRunner systemRunner) : base(machineContext)
        {
            if (systemRunner == null)
            {
                throw new ArgumentNullException(nameof(systemRunner), "SystemRunner cannot be null");
            }

            initiativeStateMachineCore = new InitiativeStateMachineCore(this, config, systemRunner);
        }

        protected override void OnDisposed()
        {
            Stop();
            initiativeStateMachineCore.Dispose();
        }

        public void Run()
        {
            initiativeStateMachineCore.Run();
        }

        public void Stop(bool terminateCurrentState = true)
        {
            initiativeStateMachineCore.Stop(terminateCurrentState);
        }
    }
}