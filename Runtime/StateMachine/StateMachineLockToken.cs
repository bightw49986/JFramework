/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.StateMachine
{
    public struct StateMachineLockToken : IEquatable<StateMachineLockToken>
    {
        public readonly int ID;
        private static int serialNumber = 0;

        private StateMachineLockToken(int id)
        {
            ID = id;
        }

        public static StateMachineLockToken Create()
        {
            int id = serialNumber++;
            return new StateMachineLockToken(id);
        }

        public bool Equals(StateMachineLockToken other)
        {
            return this.ID == other.ID;
        }
    }
}

