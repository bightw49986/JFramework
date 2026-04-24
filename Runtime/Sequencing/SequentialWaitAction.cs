/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class SequentialWaitAction : SequenceAction
    {
        [Tooltip("等待時間（秒）")]
        [InfoBox("等待時間，單位為秒")]
        [SerializeField]
        private FloatReference waitTime = new FloatReference();

        [SerializeField]
        private ScriptableDouble clock;

        [SerializeField]
        private ScriptableDouble offset;

        [SerializeField]
        private ScriptableDouble expectClockTimeOnStart;

        private double expectEndTime;

        public override void OnActionStart()
        {
            var startOffset = clock.Value - expectClockTimeOnStart.Value;
            expectEndTime = clock.Value + waitTime.Value - (startOffset + offset.Value);
            Debug.Log($"[Wait {waitTime.Value:F2}s] Next clock time: {expectEndTime:F4}", gameObject);
        }

        public override IEnumerator SequenceRoutine()
        {
            while (clock.Value < expectEndTime)
            {
                yield return null;
            }
            var actualEndTime = clock.Value;
            offset.Value = actualEndTime - expectEndTime;
            expectClockTimeOnStart.Value = actualEndTime;
            Debug.Log($"[Wait {waitTime.Value:F2}s] End at: {clock.Value:F4}, offset: {offset.Value:F4}");
        }
    }
}
