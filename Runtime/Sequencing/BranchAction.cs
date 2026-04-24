/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Collections;
using JFramework.SOAP;

namespace JFramework.Sequencing
{
    public class BranchAction : SequenceAction
    {
        [SerializeField]
        private SequenceAction trueAction;

        [SerializeField]
        private SequenceAction falseAction;

        [SerializeField]
        private BoolReference condition;

        [Required]
        [SerializeField]
        private Sequencer sequencer;

        public override IEnumerator SequenceRoutine()
        {
            if (condition)
            {
                if (trueAction != null)
                {
                    sequencer.NotifyActionStart(trueAction);
                    trueAction.OnActionStart();
                    yield return trueAction.SequenceRoutine();
                    trueAction.OnActionEnd();
                }
            }
            else
            {
                if (falseAction != null)
                {
                    sequencer.NotifyActionStart(falseAction);
                    falseAction.OnActionStart();
                    yield return falseAction.SequenceRoutine();
                    falseAction.OnActionEnd();
                }
            }
        }
    }
}
