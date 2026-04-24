/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace JFramework.Sequencing
{
    public class Sequencer : MonoBehaviour
    {
        public UnityEvent OnSequenceStart = new UnityEvent();
        public UnityEvent OnSequenceStopped = new UnityEvent();
        public UnityEvent OnSequenceComplete = new UnityEvent();
        public UnityEvent<SequenceAction> OnActionStart = new UnityEvent<SequenceAction>();

        [SerializeField]
        private List<SequenceAction> actions;

        private Coroutine sequenceCoroutine;

        private bool forceNextRequested = false;
        private SequenceAction currentAction;

        private bool isInitialized = false;

        public List<SequenceAction> Actions
        {
            get => actions;
            set
            {
                StopCurrentSequence();
                actions = value;
                if (Application.isPlaying && enabled)
                    StartSequence();
            }
        }

        private void Start()
        {
            StartSequence();
            isInitialized = true;
        }

        private void OnEnable()
        {
            if (sequenceCoroutine == null && isInitialized)
            {
                StartSequence();
            }
        }

        private void OnDisable()
        {
            StopCurrentSequence();
        }

        private void StopCurrentSequence()
        {
            if (sequenceCoroutine != null)
            {
                if (currentAction != null)
                {
                    currentAction.OnActionEnd();
                    currentAction = null;
                }
                StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;

                OnSequenceStopped?.Invoke();
            }
        }

        private void StartSequence()
        {
            sequenceCoroutine = StartCoroutine(SequenceRoutine());

            IEnumerator SequenceRoutine()
            {
                try
                {
                    OnSequenceStart?.Invoke();

                    foreach (var action in actions)
                    {
                        if (action == null)
                            continue;

                        OnActionStart?.Invoke(action);
                        currentAction = action;
                        action.OnActionStart();

                        var routine = action.SequenceRoutine();
                        while (true)
                        {
                            if (forceNextRequested)
                            {
                                forceNextRequested = false;
                                break;
                            }
                            if (!routine.MoveNext())
                            {
                                break;
                            }
                            yield return routine.Current;
                        }

                        action.OnActionEnd();
                    }

                    OnSequenceComplete?.Invoke();
                }
                finally
                {
                    sequenceCoroutine = null;
                    currentAction = null;
                }
            }
        }

        public void ForceNextAction()
        {
            // 強制跳過當前 Action 並繼續 Sequence
            if (sequenceCoroutine != null)
            {
                forceNextRequested = true;
            }
        }

        public void ForceCompleteSequence()
        {
            StopCurrentSequence();
            OnSequenceComplete?.Invoke();
        }

        public void RefreshAllActions()
        {
            var existingActions = GetComponentsInChildren<SequenceAction>();
            actions = (from act in existingActions
                       where act != null && act.gameObject.activeSelf && act.transform.parent == transform // Only include direct children
                       select act).ToList();
        }
        
        internal void NotifyActionStart(SequenceAction action)
        {
            OnActionStart?.Invoke(action);
        }
    }
}
