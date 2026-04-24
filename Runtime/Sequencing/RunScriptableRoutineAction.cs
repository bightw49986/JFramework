/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class RunScriptableRoutineAction : SequenceAction
    {
        [Required]
        [SerializeField]
        private ScriptableRoutine routine;

        [SerializeField]
        private bool waitForCompletion = true;

        public override IEnumerator SequenceRoutine()
        {
            var routineEnumerator = routine.Execute();
            if (waitForCompletion)
            {
                yield return routineEnumerator;
            }
        }
    }
}
