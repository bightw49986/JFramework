/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿#if TMP_PRESENT || TMP_IS_BUILTIN
using TMPro;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace JFramework.UI
{
    /// <summary>
    /// Chain multiple inputfields and provide functionality to auto navigate back and forth.
    /// </summary>
    public class ChainedInputFields : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Drag in input fields in the order you wish to chain them.")]
        private List<TMP_InputField> inputFieldReferences;

        [Space(15)]

        /// <summary>
        /// Invokes whenever we try to select PREVIOUS input field but can't find one, argument is current text content.
        /// </summary>
        public UnityEvent<string> ReachedStart;

        /// <summary>
        /// Invokes whenever we try to select NEXT input field but can't find one, argument is current text content.
        /// </summary>
        public UnityEvent<string> ReachedEnd;

        public string TextContent
        {
            get
            {
                var stringBuilder = new StringBuilder();
                foreach (var inputField in inputFields_Chained)
                {
                    stringBuilder.Append(inputField.text);
                }

                return stringBuilder.ToString();
            }
        }

        private Action Disabled;
        private int hitBackKeyTime;
        private LinkedList<TMP_InputField> inputFields_Chained;

        private void OnEnable()
        {
            inputFields_Chained = new LinkedList<TMP_InputField>(inputFieldReferences);

            foreach (var inputField in inputFields_Chained)
            {
                SetupInputFieldOperations(inputField);
            }
        }

        private void OnDisable()
        {
            Disabled?.Invoke();
        }

        private void SetupInputFieldOperations(TMP_InputField inputField)
        {
            inputField.onSelect.AddListener(OnSelected);
            inputField.onDeselect.AddListener(OnDeSelected);

            Disabled += OnTerminate;

            void OnSelected(string arg)
            {
                inputField.onValueChanged.AddListener(OnValueChanged);
            }

            void OnDeSelected(string arg)
            {
                inputField.onValueChanged.RemoveListener(OnValueChanged);
            }

            void OnValueChanged(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    ForwardOperation(inputField, value);
                }
                else
                {
                    BackwardOperation(inputField);
                }
            }

            void OnTerminate()
            {
                inputField.onSelect.RemoveListener(OnSelected);
                inputField.onDeselect.RemoveListener(OnDeSelected);

                Disabled -= OnTerminate;
            }
        }

        private void ForwardOperation(TMP_InputField inputField, string text)
        {
            hitBackKeyTime = 0;

            if (text.Length >= inputField.characterLimit)
            {
                var next = GetNext(inputField);
                if (next)
                {
                    next.Select();
                }
                else
                {
                    ReachedEnd?.Invoke(TextContent);
                }
            }
        }

        private void BackwardOperation(TMP_InputField inputField)
        {
            hitBackKeyTime++;

            if (hitBackKeyTime > 1)
            {
                var previous = GetPrevious(inputField);
                if (previous)
                {
                    previous.Select();
                }
                else
                {
                    ReachedStart?.Invoke(TextContent);
                }
            }
        }

        private TMP_InputField GetNext(TMP_InputField current)
        {
            var node = GetNode(current);
            return node?.Next?.Value;
        }

        private TMP_InputField GetPrevious(TMP_InputField current)
        {
            var node = GetNode(current);
            return node?.Previous?.Value;
        }

        private LinkedListNode<TMP_InputField> GetNode(TMP_InputField inputField)
        {
            return inputFields_Chained.Find(inputField);
        }
    }

}
#endif