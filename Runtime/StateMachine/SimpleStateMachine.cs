/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.StateMachine
{
    /// <summary>
    /// Simple finite state machine implementation provides functionality to manually change state and update.
    /// </summary>
    /// <typeparam name="TState">Indicates the state type this state machine can run.</typeparam>
    public class SimpleStateMachine<TState> : IStateMachine<TState> where TState : class, IState
    {
        protected IState CurrentState { get; private set; }

        private bool IsLocked => lockToken != null && lockToken.HasValue;
        private StateMachineLockToken? lockToken;

        // Cache infos for defered state change.
        private Type nextStateTypeCache;
        private object changeStateArgCache;

        private bool disposedValue;

        #region Constructors
        public SimpleStateMachine() { }

        public SimpleStateMachine(Type initialStateType, object initialChangeStateArg = null)
        {
            // Change state directly if initial state type specified.
            SetNextState(initialStateType, initialChangeStateArg, deferToNextStateUpdate: false);
        }
        #endregion

        #region IStateMachine
        public bool SetNextState<TDerivedState>(object arg = null, bool deferToNextStateUpdate = true) where TDerivedState : TState, new()
        {
            return SetNextState(typeof(TDerivedState), arg, deferToNextStateUpdate);
        }

        public bool SetNextState(Type type, object arg = null, bool deferToNextStateUpdate = true)
        {
            if (IsLocked)
            {
                Debug.LogWarning($"SetNextState(): failed, currently locked.");
                return false;
            }

            bool targetStateValid = false;
            if (type == null || typeof(TState).IsAssignableFrom(type))
            {
                if (CurrentState != null)
                {
                    targetStateValid = CurrentState.CanChangeTo(type);
                    if (!targetStateValid)
                    {
                        Debug.LogWarning($"SetNextState(): failed, denied by current state.");
                        return false;
                    }
                }
                nextStateTypeCache = type;
                changeStateArgCache = arg;
                targetStateValid = true;

                if (!deferToNextStateUpdate)
                {
                    PerformStateChange(changeStateArgCache);
                }
            }
            else
            {
                throw new StateMachineException(CurrentState, "SetNextState(): failed: given type is not a type of IState");
            }
            return targetStateValid;
        }

        public void UpdateCurrentState(object arg = null)
        {
            if (null != nextStateTypeCache) // Perform state change if specified.
            {
                PerformStateChange(changeStateArgCache);
            }
            else  // simply update current state.
            {
                CurrentState?.Update(this, arg);
            }
        }

        public bool Lock(out StateMachineLockToken token)
        {
            token = default;
            if (IsLocked)
            {
                Debug.LogWarning($"Lock(): failed, already locked.");
                return false;
            }

            token = StateMachineLockToken.Create();
            lockToken = token;
            return true;
        }

        public bool UnLock(StateMachineLockToken token)
        {
            if (!IsLocked)
            {
                Debug.LogWarning($"UnLock(): failed, currently not locked.");
                return false;
            }
            if (!(token.Equals(lockToken.Value)))
            {
                Debug.LogWarning($"UnLock(): failed, token id not match, was:{lockToken.Value.ID}, given:{token.ID}.");
                return false;
            }

            lockToken = null;
            return true;
        }
        #endregion

        private void PerformStateChange(object arg)
        {
            if (IsLocked)
            {
                Debug.LogWarning($"PerformStateChange(): failed, currently locked.");
                return;
            }

            var nextStateExist = nextStateTypeCache != null;
            var currentStateName = (CurrentState?.GetType())?.ToString();
            var targetStateName = nextStateTypeCache?.ToString();

            if (nextStateExist) // Perform change to newState.
            {
                if (!(Activator.CreateInstance(nextStateTypeCache) is IState newState))
                {
                    throw new StateMachineException(CurrentState, $"PerformStateChange(): failed, Next state type:{nextStateTypeCache} is not a IState!");
                }
                OnStateCreated(newState);
                CurrentState?.Terminate(this, arg);
                newState.Initialize(this, arg);
                CurrentState = newState;
            }
            else // Pefrom change state to null.
            {
                CurrentState?.Terminate(this, arg);
                CurrentState = null;
            }

            Debug.Log($"PerformStateChange(): completed, form:{currentStateName} -> to:{targetStateName}");

            // Clear the change state cache.
            nextStateTypeCache = null;
            changeStateArgCache = null;
        }

        protected virtual void OnStateCreated(IState newState) { }

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                nextStateTypeCache = null;
                changeStateArgCache = null;
                lockToken = null;

                CurrentState?.Terminate(this);
                CurrentState = null;

                if (disposing)
                {
                    OnDisposed();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDisposed() { }
        #endregion
    }

    /// <summary>
    /// Simple finite state machine implementation with defined context provides functionality to manually change state and update.
    /// </summary>
    /// <typeparam name="TState">Indicates the state type this state machine can run.</typeparam>
    /// <typeparam name="TContext">The context object this machine share around all the states.</typeparam>
    public class SimpleStateMachine<TState, TContext> : SimpleStateMachine<TState> where TState : class, IState<TContext>
    {
        protected readonly TContext machineContext;

        public SimpleStateMachine(TContext machineContext) : base()
        {
            this.machineContext = machineContext;
        }

        public SimpleStateMachine(TContext machineContext, Type initialStatType, object arg = null)
        : base(initialStatType, arg)
        {
            this.machineContext = machineContext;
        }

        protected override void OnStateCreated(IState newState)
        {
            if (newState is IState<TContext> stateWithContext)
            {
                stateWithContext.RunnerContext = machineContext;
            }
        }
    }
}

