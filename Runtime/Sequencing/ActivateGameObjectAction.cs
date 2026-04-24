/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using JFramework.SOAP;
using System.Collections;

namespace JFramework.Sequencing
{
    public class ActivateGameObjectAction : SequenceAction
    {
        [SerializeField]
        private GameObject targetObject;

        [SerializeField]
        private BoolReference setActive = new BoolReference(true);

        public override IEnumerator SequenceRoutine()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(setActive);
            }
            yield break;
        }
    }
}
