/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if USE_NEW_INPUT_SYSTEM
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace JFramework.Sequencing
{
    public class WaitForAnyButtonNewInputAction : SequenceAction
    {
        private bool buttonPressed = false;
        private IDisposable observable;

        public override void OnActionStart()
        {
            buttonPressed = false;
            observable = InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPress);
            Debug.Log("[WaitForAnyButtonNewInputAction] Started Waiting for Any Button Press");
        }

        private void OnAnyButtonPress(InputControl _)
        {
            buttonPressed = true;
            Debug.Log("[WaitForAnyButtonNewInputAction] Button Pressed");
        }

        public override IEnumerator SequenceRoutine()
        {
            while (!buttonPressed)
            {
                yield return null;
            }
        }

        public override void OnActionEnd()
        {
            observable?.Dispose();
        }
    }
}
#endif