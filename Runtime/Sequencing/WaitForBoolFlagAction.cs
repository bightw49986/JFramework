/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Collections;
using JFramework.SOAP;

namespace JFramework.Sequencing
{
    public class WaitForBoolFlagAction : SequenceAction
    {
        [SerializeField]
        private ScriptableBool boolFlag;

        [SerializeField]
        private bool reverseCondition;

        public override IEnumerator SequenceRoutine()
        {
            while (boolFlag.Value == reverseCondition)
            {
                yield return null;
            }
        }
    }
}
