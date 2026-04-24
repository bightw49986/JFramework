/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using UnityEngine.UI;

namespace JFramework.Binding
{
    [RequireComponent(typeof(Text))]
    public class TextBinder : TextBinderBase
    {
        [Required]
        [SerializeField]
        private Text targetText;

        protected override void SetTextWithCurrentValue()
        {
            switch (bindingSource)
            {
                case BindingSource.String:
                    targetText.text = string.Concat(prefix, stringSource.Value, suffix);
                    break;
                case BindingSource.Int:
                    targetText.text = string.Concat(prefix, intSource.Value.ToString(numberFormat), suffix);
                    break;
                case BindingSource.Float:
                    targetText.text = string.Concat(prefix, floatSource.Value.ToString(numberFormat), suffix);
                    break;
            }
        }

        protected override void SetTextDirectly(string text)
        {
            targetText.text = string.Concat(prefix, text, suffix);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (targetText == null)
            {
                targetText = GetComponent<Text>();
            }
        }
#endif
    }
}