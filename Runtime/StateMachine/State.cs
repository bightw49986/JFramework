/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.StateMachine
{
    /// <summary>
    /// Base implementation of <seealso cref="IState"/>
    /// </summary>
    public abstract class State : IState
    {
        private IStateMachine runner;
        private StateMachineLockToken lockToken;

        #region IState callbacks
        public void Initialize(IStateMachine runner, object arg = null)
        {
            this.runner = runner;
            OnInit(arg);
        }
        protected virtual void OnInit(object arg = null) { }

        public void Update(IStateMachine runner, object arg = null)
        {
            OnUpdate(arg);
        }
        protected virtual void OnUpdate(object arg = null) { }

        public void Terminate(IStateMachine runner, object arg = null)
        {
            OnTerminated(arg);
        }
        protected virtual void OnTerminated(object arg = null) { }

        public virtual bool CanChangeTo(Type targetStateType)
        {
            return targetStateType == null || typeof(IState).IsAssignableFrom(targetStateType);
        }
        #endregion

        #region  In-state runner function wrappers
        protected bool ChangeState(Type type, object arg = null, bool deferToNextStateUpdate = true)
        {
            return runner.SetNextState(type, arg, deferToNextStateUpdate);
        }

        protected bool ChangeState<TState>(object arg = null, bool deferToNextStateUpdate = true) where TState : State, new()
        {
            if (!(runner is IStateMachine<TState> specifiedStateRunner))
            {
                throw new StateMachineException(this, $"Can't change to state:{typeof(TState)}: runner is not capible for handling this type of state.");
            }

            return specifiedStateRunner.SetNextState<TState>(arg, deferToNextStateUpdate);
        }

        protected bool Lock()
        {
            return runner.Lock(out lockToken);
        }

        protected bool UnLock()
        {
            return runner.UnLock(lockToken);
        }
        #endregion        
    }

    /// <summary>
    /// Base implementation of <see cref="IState{TContext}"/>
    /// </summary>
    /// <typeparam name="TContext">Type of custom shared context.</typeparam>
    public abstract class State<TContext> : State, IState<TContext>
    {
        protected TContext runnerContext;

        public TContext RunnerContext { set => runnerContext = value; }

        public override bool CanChangeTo(Type targetStateType)
        {
            return targetStateType == null || typeof(IState<TContext>).IsAssignableFrom(targetStateType);
        }
    }
}

