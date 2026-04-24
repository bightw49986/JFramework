/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if USE_NEW_INPUT_SYSTEM
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JFramework.Sequencing
{
    public class WaitForInputAction : SequenceAction
    {
        [SerializeField]
        private InputActionReference[] inputActionReference;

        private bool inputPerformed = false;

        public override void OnActionStart()
        {
            inputPerformed = false;

            foreach (var actionRef in inputActionReference)
            {
                if (actionRef != null && actionRef.action != null)
                {
                    actionRef.action.performed += OnInputPerformed;
                }
            }

            Debug.Log($"[WaitForInputAction] Started Waiting for Inputs: {string.Join(", ", inputActionReference.Select(actionRef => actionRef.action.name))}");
        }

        public override IEnumerator SequenceRoutine()
        {
            while (!inputPerformed)
            {
                yield return null;
            }
        }

        public override void OnActionEnd()
        {
            foreach (var actionRef in inputActionReference)
            {
                if (actionRef != null && actionRef.action != null)
                {
                    actionRef.action.performed -= OnInputPerformed;
                }
            }
        }

        private void OnInputPerformed(InputAction.CallbackContext context)
        {
            inputPerformed = true;
            Debug.Log("[WaitForInputAction] Input Performed: " + context.action.name);
        }

    }
}
#endif