/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

namespace JFramework.Sequencing
{
    public class PlayTimelineAction : SequenceAction
    {
        [Required]
        [SerializeField]
        private PlayableDirector timeline;

        [SerializeField]
        private bool waitForCompletion = true;

        public override void OnActionStart()
        {
            if (!timeline.gameObject.activeSelf)
                timeline.gameObject.SetActive(true);

            timeline.Play();
            Debug.Log($"[PlayTimelineAction] Play timeline on {timeline.gameObject}", timeline);
        }

        public override IEnumerator SequenceRoutine()
        {
            if (waitForCompletion)
            {
                while (timeline.state == PlayState.Playing)
                {
                    yield return null;
                }
            }
        }
    }
}
