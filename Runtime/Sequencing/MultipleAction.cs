/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Collections;

namespace JFramework.Sequencing
{
    public class MultipleAction : SequenceAction
    {
        [SerializeField]
        private SequenceAction[] actions;

        [Required]
        [SerializeField]
        private Sequencer sequencer;

        public override IEnumerator SequenceRoutine()
        {
            foreach (var action in actions)
            {
                if (action != null)
                {
                    sequencer.NotifyActionStart(action);
                    action.OnActionStart();
                    yield return action.SequenceRoutine();
                    action.OnActionEnd();
                }
            }
        }

#if UNITY_EDITOR
            private void Reset()
            {
                if (sequencer == null)
                    sequencer = GetComponentInParent<Sequencer>();
            }
#endif
    }
}
