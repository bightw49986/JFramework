/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class SetAnimatorStateAction : SequenceAction
    {
        [Required]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private StringReference stateName;

        [SerializeField]
        private IntReference layer = new IntReference(0);

        [SerializeField]
        private FloatReference normalizedTime = new FloatReference(0f);

        [SerializeField]
        private bool waitForCompletion = true;

        [Range(0f, 1f)]
        [SerializeField]
        private float completeEarlyInNormalizedTime = 0.95f;

        [SerializeField]
        private bool closeObjectOnComplete = true;

        public override void OnActionStart()
        {
            animator.Play(stateName, layer, normalizedTime);
            Debug.Log("[SetAnimatorState] To " + stateName);
        }

        public override IEnumerator SequenceRoutine()
        {
            if (waitForCompletion)
            {
                yield return null; // Wait one frame to ensure the state has changed.

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < completeEarlyInNormalizedTime)
                {
                    stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    yield return null;
                }
                if (closeObjectOnComplete)
                {
                    animator.gameObject.SetActive(false);
                }
            }
        }
    }
}
