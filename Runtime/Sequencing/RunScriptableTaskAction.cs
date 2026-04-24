/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class RunScriptableTaskAction : SequenceAction
    {
        [Required]
        [SerializeField]
        private ScriptableTask task;

        [SerializeField]
        private bool waitForCompletion = true;

        public override IEnumerator SequenceRoutine()
        {
            var taskCompletion = task.Execute();
            while (waitForCompletion && !taskCompletion.IsCompleted)
            {
                yield return null;
            }
        }
    }
}
