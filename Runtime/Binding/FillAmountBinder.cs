/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.UI;
using JFramework.SOAP;

namespace JFramework.Binding
{
    public class FillAmountBinder : MonoBehaviour
    {
        [Tooltip("The type of binding behaviour. When set to 'Update', the fill amount will be updated every frame. When set to 'OnValueChanged', the fill amount will only update when the source value changes.")]
        [SerializeField]
        private BindingType bindingType = BindingType.Update;

        [SerializeField]
        private Image image;

        [SerializeField]
        private ScriptableFloat fillAmount;

        [SerializeField]
        private bool setFillAtEnable = true;

        private void OnEnable()
        {
            if (bindingType == BindingType.OnValueChanged)
            {
                fillAmount.ValueChanged += OnValueChanged;
            }
            if (setFillAtEnable)
            {
                image.fillAmount =  Mathf.Clamp01(fillAmount);
            }
        }

        private void OnDisable()
        {
            if (bindingType == BindingType.OnValueChanged)
            {
                fillAmount.ValueChanged -= OnValueChanged;
            }
        }

        private void Update()
        {
            if (bindingType != BindingType.Update)
                return;

            image.fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void OnValueChanged(float newValue, float oldValue)
        {
            image.fillAmount = Mathf.Clamp01(newValue);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
        }
#endif
    }
}
