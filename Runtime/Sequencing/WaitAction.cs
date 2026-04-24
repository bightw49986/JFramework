/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class WaitAction : SequenceAction
    {
        [Tooltip("等待時間（秒）")]
        [InfoBox("等待時間，單位為秒")]
        [SerializeField]
        private FloatReference waitTime = new FloatReference();

        public override IEnumerator SequenceRoutine()
        {
            Debug.Log($"[Wait {waitTime:F4}s] Started at {Time.time:F4}", gameObject);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
