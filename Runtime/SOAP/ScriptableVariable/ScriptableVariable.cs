/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace JFramework.SOAP
{
    public delegate void ValueChangedHandler<T>(T newValue, T oldValue);

    public abstract class ScriptableVariable<T> : ScriptableObject where T : IEquatable<T>
    {
        public event ValueChangedHandler<T> ValueChanged;

#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        private string description;
#endif

        [SerializeField]
        private T value;

        public T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    T oldValue = this.value;
                    this.value = value;
                    ValueChanged?.Invoke(value, oldValue);
                }
            }
        }

#if UNITY_EDITOR
        private T editorInitialValue;
#endif

        internal void ClearValueChangedSubscribers()
        {
            ValueChanged = null;
        }

        
#if UNITY_EDITOR
        private void OnEnable()
        {
            editorInitialValue = value;
        }
#endif

        private void OnDisable()
        {
            ClearValueChangedSubscribers();
            
#if UNITY_EDITOR
            // 在編輯器中，當 ScriptableObject 被禁用（通常是因為場景切換或編輯器關閉）時，重置值為初始值
            value = editorInitialValue;
#endif
        }

        #region Operators
        public static implicit operator T(ScriptableVariable<T> variable)
        {
            return variable.Value;
        }
        #endregion
    }
}

