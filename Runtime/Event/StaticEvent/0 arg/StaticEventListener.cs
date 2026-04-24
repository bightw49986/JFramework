/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace JFramework.SOAP.Event
{
    public class StaticEventListener : MonoBehaviour
    {
#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        private string description;
#endif

        [SerializeField]
        private StaticEvent staticEvent;

        [SerializeField]
        protected bool raiseUnityEvent = true;

        [ShowIf(nameof(raiseUnityEvent))]
        [SerializeField]
        private UnityEvent onEventRaised = new UnityEvent();

        private void OnEnable()
        {
            if (staticEvent == null)
            {
                Debug.LogError($"No static event serialized on:{this}!", gameObject);
                return;
            }

            staticEvent.AddListener(HandleEvent);
        }

        private void OnDisable()
        {
            if (staticEvent == null)
                return;

            staticEvent.RemoveListener(HandleEvent);
        }

        private void HandleEvent(object sender, EventArgs args)
        {
            if (raiseUnityEvent)
                onEventRaised?.Invoke();

            OnEventRaised(sender, args);
        }

        protected virtual void OnEventRaised(object sender, EventArgs e) { }
    }
}


