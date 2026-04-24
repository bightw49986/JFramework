/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.StateMachine
{
    /// <summary>
    /// Provides basic functionality to act like a finite state machine.
    /// </summary>
    public interface IStateMachine : IDisposable
    {
        /// <summary>
        /// Perform a update to current state.
        /// </summary>
        /// <param name="arg">Custom update argument.</param>
        void UpdateCurrentState(object arg = null);

        /// <summary>
        /// Try change to a specified state of type.
        /// </summary>
        /// <param name="type">Type of target state.</param>
        /// <param name="arg">Custom change state argument.</param>
        /// <param name="deferToNextStateUpdate">Defer actual state change to next state update, it's highly recommanded to do this to sync up all the possible change timing.</param>
        /// <returns>True if change state accepted.</returns>
        bool SetNextState(Type type, object arg = null, bool deferToNextStateUpdate = true);
        
        /// <summary>
        /// Lock the machine to avoid any potential state changes.
        /// </summary>
        /// <param name="token">Keep this token and use it to unlock.</param>
        /// <returns>Lock successfully.</returns>
        bool Lock(out StateMachineLockToken token);

        /// <summary>
        /// Unlock the machine to accept further state changes.
        /// </summary>
        /// <param name="token">The same token that user used to lock previously.</param>
        /// <returns>Unlock successfully.</returns>
        bool UnLock(StateMachineLockToken token);
    }

    /// <summary>
    /// Provides basic functionality to be a finite state machine which can handle specified type of <seealso cref="IState"/>.
    /// </summary>
    /// <typeparam name="TState">Specified the base type of state that this machine handles.</typeparam>
    public interface IStateMachine<TState> : IStateMachine where TState : class, IState
    {
        /// <summary>
        /// Try change to a specified state of type.
        /// </summary>
        /// <typeparam name="TDerivedState">Specify state type</typeparam>
        /// <param name="arg">Custom change state argument.</param>
        /// <param name="deferToNextStateUpdate">Defer actual state change to next state update, it's highly recommanded to do this to sync up all the possible change timing.</param>
        /// <returns>True if change state accepted.</returns>
        bool SetNextState<TDerivedState>(object arg = null, bool deferToNextStateUpdate = true) where TDerivedState : TState, new();
    }
}

