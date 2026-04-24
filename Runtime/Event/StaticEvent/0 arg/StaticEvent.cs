/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP.Event
{
    [CreateAssetMenu(menuName = "JFramework/Event/0 arg")]
    public class StaticEvent : ScriptableObject
    {
#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        private string description;
#endif

        public event EventHandler EventLaunched;

        /// <summary>
        /// Raise the event.
        /// </summary>
        /// <param name="sender">Event sender</param>
        public void Raise(object sender)
        {
            EventLaunched?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Raise the event with custom args
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        public void Raise(object sender, EventArgs args)
        {
            EventLaunched?.Invoke(sender, args);
        }

        /// <summary>
        /// Register to the event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        /// <returns>Event handler passed in, useful when using annoymous method for later un-registration.</returns>
        public EventHandler AddListener(EventHandler handler)
        {
            EventLaunched += handler;
            return handler;
        }

        /// <summary>
        /// Unregister from the event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        public void RemoveListener(EventHandler handler)
        {
            EventLaunched -= handler;
        }
    }

    /// <summary>
    /// Abstract base class for all generic StaticEvent<T> types.
    /// This allows us to create a targeted CustomEditor without affecting all ScriptableObjects.
    /// </summary>
    public abstract class GenericStaticEventBase : ScriptableObject
    {

    }

    public abstract class StaticEvent<T> : GenericStaticEventBase
    {
#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        protected string description;
#endif

        public event EventHandler<T> EventLaunched;

        [SerializeField]
        private T testValue;

        /// <summary>
        /// Raise the event.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        public void Raise(object sender, T args)
        {
            EventLaunched?.Invoke(sender, args);
        }

        /// <summary>
        /// Register to the event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        /// <returns>Event handler passed in, useful when using annoymous method for later un-registration.</returns>
        public EventHandler<T> AddListener(EventHandler<T> handler)
        {
            EventLaunched += handler;
            return handler;
        }

        /// <summary>
        /// Unregister from the event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        public EventHandler<T> RemoveListener(EventHandler<T> handler)
        {
            EventLaunched -= handler;
            return handler;
        }
    }
}


