/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using UnityEngine;
using JFramework.UI;
using JFramework.SOAP;
using System.Collections.Generic;

namespace JFramework.Binding
{
    public abstract class TextBinderBase : MonoBehaviour
    {
        [Tooltip("The type of binding behaviour. When set to 'Update', the text will be updated every frame. When set to 'OnValueChanged', the text will only update when the source value changes.")]
        [SerializeField]
        protected BindingType bindingType = BindingType.Update;

        [Tooltip("The type of source to bind the text to.")]
        [SerializeField]
        protected BindingSource bindingSource = BindingSource.String;

        [Required]
        [SerializeField]
        protected ScriptableString stringSource;

        [Required]
        [SerializeField]
        protected ScriptableInt intSource;

        [Required]
        [SerializeField]
        protected ScriptableFloat floatSource;

        [Tooltip("The format string used to format the text. Use '{0}' as a placeholder for the value.")]
        [SerializeField]
        protected string textFormat = "{0}";

        [Tooltip("The format string pass into ToString() when formatting the value.")]
        [SerializeField]
        protected string numberFormat = string.Empty;

        [Tooltip("If true, the text will be set immediately when the component is enabled.")]
        [SerializeField]
        protected bool setTextAtEnable = true;

        [Tooltip("The text effect to apply when the text change.")]
        [SerializeField]
        protected TextEffectBase textEffect;

        protected readonly Dictionary<BindingSource, Action> SubscribeActions = new Dictionary<BindingSource, Action>();
        protected readonly Dictionary<BindingSource, Action> UnsubscribeActions = new Dictionary<BindingSource, Action>();

        protected string lastStringValue;
        protected int? lastIntValue;
        protected float? lastFloatValue;

        protected string prefix;
        protected string suffix;

        #region MonoBehaviours
        protected virtual void Awake()
        {
            SubscribeActions.Add(BindingSource.String, SubscribeString);
            SubscribeActions.Add(BindingSource.Int, SubscribeInt);
            SubscribeActions.Add(BindingSource.Float, SubscribeFloat);

            UnsubscribeActions.Add(BindingSource.String, UnsubscribeString);
            UnsubscribeActions.Add(BindingSource.Int, UnsubscribeInt);
            UnsubscribeActions.Add(BindingSource.Float, UnsubscribeFloat);

            void SubscribeString()
            {
                stringSource.ValueChanged += OnStringSourceValueChanged;
            }

            void SubscribeInt()
            {
                intSource.ValueChanged += OnIntSourceValueChanged;
            }

            void SubscribeFloat()
            {
                floatSource.ValueChanged += OnFloatSourceValueChanged;
            }

            void UnsubscribeString()
            {
                stringSource.ValueChanged -= OnStringSourceValueChanged;
            }

            void UnsubscribeInt()
            {
                intSource.ValueChanged -= OnIntSourceValueChanged;
            }

            void UnsubscribeFloat()
            {
                floatSource.ValueChanged -= OnFloatSourceValueChanged;
            }
        }

        protected virtual void OnEnable()
        {
            UpdatePrefixAndSuffix();
            SubscribeToValueChanged(bindingType, bindingSource);

            if (setTextAtEnable)
            {
                InitiallySetText();
            }

            void UpdatePrefixAndSuffix()
            {
                var index = textFormat.IndexOf("{0}", StringComparison.Ordinal);
                if (index < 0)
                {
                    prefix = textFormat;
                    suffix = string.Empty;
                }
                else
                {
                    prefix = textFormat.Substring(0, index);
                    suffix = textFormat.Substring(index + 3);
                }
            }

            void SubscribeToValueChanged(BindingType type, BindingSource source)
            {
                if (type != BindingType.OnValueChanged)
                    return;

                if (!SubscribeActions.TryGetValue(source, out var action))
                {
                    Debug.LogError($"[{GetType().Name}] No subscribe action found for binding source: {source}");
                    return;
                }
                action.Invoke();
            }

            void InitiallySetText()
            {
                if (bindingType == BindingType.OnValueChanged && textEffect != null)
                {
                    switch (bindingSource)
                    {
                        case BindingSource.String:
                            var stringContext = CreateTextEffectContext(stringSource.Value, default);
                            textEffect.HandleData(stringContext);
                            return;
                        case BindingSource.Int:
                            if (textEffect is not IDataHandler<TextEffectContext<int>> intDataHandler)
                            {
                                Debug.LogError($"[{GetType().Name}] TextEffect does not support int type context.", textEffect);
                                break;
                            }
                            var intContext = CreateTextEffectContext(intSource.Value, default);
                            intDataHandler.HandleData(intContext);
                            return;
                        case BindingSource.Float:
                            if (textEffect is not IDataHandler<TextEffectContext<float>> floatDataHandler)
                            {
                                Debug.LogError($"[{GetType().Name}] TextEffect does not support float type context.", textEffect);
                                break;
                            }
                            var floatContext = CreateTextEffectContext(floatSource.Value, default);
                            floatDataHandler.HandleData(floatContext);
                            return;
                    }
                }

                SetTextWithCurrentValue();
            }
        }

        protected virtual void Update()
        {
            if (bindingType != BindingType.Update)
                return;

            UpdateRefreshText();

            void UpdateRefreshText()
            {
                switch (bindingSource)
                {
                    case BindingSource.String:
                        if (stringSource.Value != lastStringValue)
                        {
                            lastStringValue = stringSource.Value;
                            SetTextWithCurrentValue();
                        }
                        break;
                    case BindingSource.Int:
                        if (intSource.Value != lastIntValue)
                        {
                            lastIntValue = intSource.Value;
                            SetTextWithCurrentValue();
                        }
                        break;
                    case BindingSource.Float:
                        if (floatSource.Value != lastFloatValue)
                        {
                            lastFloatValue = floatSource.Value;
                            SetTextWithCurrentValue();
                        }
                        break;
                }
            }
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromValueChanged(bindingType, bindingSource);

            void UnsubscribeFromValueChanged(BindingType type, BindingSource source)
            {
                if (type != BindingType.OnValueChanged)
                    return;

                if (!UnsubscribeActions.TryGetValue(source, out var action))
                {
                    Debug.LogError($"[{GetType().Name}] No unsubscribe action found for binding source: {source}");
                    return;
                }
                action.Invoke();
            }
        }
        #endregion

        #region Protected Methods
        protected TextEffectContext<T> CreateTextEffectContext<T>(T newValue, T oldValue)
        {
            return new TextEffectContext<T>
            (
                textFormat: textFormat,
                numberFormat: numberFormat,
                newValue: newValue,
                oldValue: oldValue
            );
        }

        protected abstract void SetTextWithCurrentValue();

        protected virtual void OnFloatSourceValueChanged(float newValue, float oldValue)
        {
            if (textEffect != null && textEffect is IDataHandler<TextEffectContext<float>> floatDataHandler)
            {
                var effectContext = CreateTextEffectContext(newValue, oldValue);
                floatDataHandler.HandleData(effectContext);
                return;
            }
            SetTextDirectly(newValue.ToString(numberFormat));
        }

        protected virtual void OnIntSourceValueChanged(int newValue, int oldValue)
        {
            if (textEffect != null && textEffect is IDataHandler<TextEffectContext<int>> intDataHandler)
            {
                var effectContext = CreateTextEffectContext(newValue, oldValue);
                intDataHandler.HandleData(effectContext);
                return;
            }
            SetTextDirectly(newValue.ToString(numberFormat));
        }

        protected virtual void OnStringSourceValueChanged(string newValue, string oldValue)
        {
            if (textEffect != null && textEffect)
            {
                var effectContext = CreateTextEffectContext(newValue, oldValue);
                textEffect.HandleData(effectContext);
                return;
            }
            SetTextDirectly(newValue);
        }

        protected abstract void SetTextDirectly(string text);
        #endregion
    }
}
