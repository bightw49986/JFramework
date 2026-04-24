/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;

namespace JFramework.SOAP
{
    public abstract class ValueReference<T, U> : IDisposable, IEquatable<T>
    where T : IEquatable<T>
    where U : ScriptableVariable<T>
    {
        [SerializeField]
        protected bool useScriptableValue;

        [ShowIf(nameof(useScriptableValue))]
        [SerializeField]
        private U scriptableValue;

        [HideIf(nameof(useScriptableValue))]
        [SerializeField]
        private T constValue;

        public T Value
        {
            get
            {
                return useScriptableValue ? scriptableValue.Value : constValue;
            }
            set
            {
                if (useScriptableValue)
                    scriptableValue.Value = value;
                else
                    constValue = value;
            }
        }

        public ValueReference() { }

        public ValueReference(T constValue) : this()
        {
            this.constValue = constValue;
        }

        public void SubscribeToValueChanged(ValueChangedHandler<T> handler)
        {
            if (useScriptableValue && scriptableValue != null)
            {
                scriptableValue.ValueChanged += handler;
            }
        }

        public void UnsubscribeFromValueChanged(ValueChangedHandler<T> handler)
        {
            if (useScriptableValue && scriptableValue != null)
            {
                scriptableValue.ValueChanged -= handler;
            }
        }

        #region Object
        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(T other)
        {
            return Value.Equals(other);
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (useScriptableValue && scriptableValue != null)
                    {
                        scriptableValue.ClearValueChangedSubscribers();
                    }
                    OnDisposed();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDisposed() { }

        private bool disposedValue;
        #endregion

        #region Operators
        public static implicit operator T(ValueReference<T, U> valueReference)
        {
            return valueReference.Value;
        }
        #endregion
    }
}
