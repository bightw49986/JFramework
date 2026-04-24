/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.StateMachine
{
    /// <summary>
    /// Provides basic functionality for an <seealso cref="IStateMachine"/> to run.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Invoked when state being first created.
        /// </summary>
        /// <param name="runner">State machine that runs this state.</param>
        /// <param name="arg">Custom createion argument.</param>
        void Initialize(IStateMachine runner, object arg = null);
        
        /// <summary>
        /// Invoked whenever the state machine performs a update.
        /// </summary>
        /// <param name="runner">State machine that runs this state.</param>
        /// <param name="arg">Custom update argument.</param>
        void Update(IStateMachine runner, object arg = null);
        
        /// <summary>
        /// Invoked when state being finalized.
        /// </summary>
        /// <param name="runner">State machine that runs this state.</param>
        /// <param name="arg">Custom terminate argument.</param>
        void Terminate(IStateMachine runner, object arg = null);
        
        /// <summary>
        /// Ask this state whether can change to specific state or not.
        /// </summary>
        /// <param name="targetStateType">Target state type user want to change to.</param>
        /// <returns>Can perform state change.</returns>
        bool CanChangeTo(Type targetStateType);
    }

    /// <summary>
    /// Provides basic functionality for an <seealso cref="IStateMachine"/> with specified context to run.
    /// </summary>
    public interface IState<TContext> : IState
    {
        /// <summary>
        /// Custom shared context inside the runner state machine domain.
        /// </summary>
        TContext RunnerContext { set; }
    }
}

