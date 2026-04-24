/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.StateMachine
{
    public class StateMachineException : Exception
    {
        public readonly string CurrentStateName;
        public StateMachineException(IState currentState) : base()
        {
            CurrentStateName = nameof(currentState);
        }

        public StateMachineException(IState currentState, string message) : base(message)
        {
            CurrentStateName = nameof(currentState);
        }

        public override string ToString()
        {
            var baseMsg = base.ToString();
            baseMsg += $",current state: {CurrentStateName}";
            return baseMsg;
        }
    }
}

