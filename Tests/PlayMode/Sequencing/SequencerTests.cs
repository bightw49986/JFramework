using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using JFramework.Sequencing;

namespace JFramework.Tests.PlayMode.Sequencing
{
    class InstantAction : SequenceAction
    {
        public bool StartCalled;
        public bool EndCalled;
        public override void OnActionStart() => StartCalled = true;
        public override IEnumerator SequenceRoutine() { yield break; }
        public override void OnActionEnd() => EndCalled = true;
    }

    class WaitFramesAction : SequenceAction
    {
        public int FramesToWait = 3;
        public override IEnumerator SequenceRoutine()
        {
            for (int i = 0; i < FramesToWait; i++) yield return null;
        }
    }

    public class SequencerTests
    {
        GameObject go;
        Sequencer sequencer;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Sequencer");
            go.SetActive(false);                              // 停用，讓 Start() 延後到測試主動啟用
            sequencer = go.AddComponent<Sequencer>();
            SetField(sequencer, "actions", new List<SequenceAction>());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        static void SetField(object obj, string name, object value)
        {
            var t = obj.GetType();
            while (t != null)
            {
                var f = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (f != null) { f.SetValue(obj, value); return; }
                t = t.BaseType;
            }
        }

        [UnityTest]
        public IEnumerator OnSequenceStart_FiredWhenSequenceBegins()
        {
            bool fired = false;
            sequencer.OnSequenceStart.AddListener(() => fired = true);

            go.SetActive(true);
            yield return null;

            Assert.IsTrue(fired, "OnSequenceStart should have been fired.");
        }

        [UnityTest]
        public IEnumerator OnSequenceComplete_FiredAfterAllActionsFinish()
        {
            var actionGo1 = new GameObject("Action1");
            actionGo1.transform.SetParent(go.transform);
            var action1 = actionGo1.AddComponent<InstantAction>();

            var actionGo2 = new GameObject("Action2");
            actionGo2.transform.SetParent(go.transform);
            var action2 = actionGo2.AddComponent<InstantAction>();

            SetField(sequencer, "actions", new List<SequenceAction> { action1, action2 });

            bool fired = false;
            sequencer.OnSequenceComplete.AddListener(() => fired = true);

            go.SetActive(true);
            yield return null;
            yield return null;
            yield return null;

            Assert.IsTrue(fired, "OnSequenceComplete should have been fired after all actions finish.");
        }

        [UnityTest]
        public IEnumerator OnActionStart_FiredForEachAction()
        {
            var actionGo1 = new GameObject("Action1");
            actionGo1.transform.SetParent(go.transform);
            var action1 = actionGo1.AddComponent<InstantAction>();

            var actionGo2 = new GameObject("Action2");
            actionGo2.transform.SetParent(go.transform);
            var action2 = actionGo2.AddComponent<InstantAction>();

            SetField(sequencer, "actions", new List<SequenceAction> { action1, action2 });

            int callCount = 0;
            sequencer.OnActionStart.AddListener(_ => callCount++);

            go.SetActive(true);
            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(2, callCount, "OnActionStart should have been fired twice.");
        }

        [UnityTest]
        public IEnumerator Actions_RunInOrder()
        {
            var actionGo1 = new GameObject("Action1");
            actionGo1.transform.SetParent(go.transform);
            var action1 = actionGo1.AddComponent<InstantAction>();

            var actionGo2 = new GameObject("Action2");
            actionGo2.transform.SetParent(go.transform);
            var action2 = actionGo2.AddComponent<InstantAction>();

            SetField(sequencer, "actions", new List<SequenceAction> { action1, action2 });

            var order = new List<SequenceAction>();
            sequencer.OnActionStart.AddListener(a => order.Add(a));

            go.SetActive(true);
            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(2, order.Count, "Should have recorded 2 actions.");
            Assert.AreEqual(action1, order[0], "First action should be action1.");
            Assert.AreEqual(action2, order[1], "Second action should be action2.");
        }

        [UnityTest]
        public IEnumerator ForceNextAction_SkipsCurrentAction()
        {
            var actionGo = new GameObject("Action");
            actionGo.transform.SetParent(go.transform);
            var action = actionGo.AddComponent<WaitFramesAction>();
            action.FramesToWait = 100;

            SetField(sequencer, "actions", new List<SequenceAction> { action });

            bool completed = false;
            sequencer.OnSequenceComplete.AddListener(() => completed = true);

            go.SetActive(true);
            yield return null;              // Start() 跑，WaitFramesAction 開始等待

            sequencer.ForceNextAction();

            yield return null;
            yield return null;
            yield return null;

            Assert.IsTrue(completed, "OnSequenceComplete should have been fired after ForceNextAction.");
        }

        [UnityTest]
        public IEnumerator ForceCompleteSequence_StopsAndFiresComplete()
        {
            var actionGo = new GameObject("Action");
            actionGo.transform.SetParent(go.transform);
            var action = actionGo.AddComponent<WaitFramesAction>();
            action.FramesToWait = 100;

            SetField(sequencer, "actions", new List<SequenceAction> { action });

            bool completed = false;
            sequencer.OnSequenceComplete.AddListener(() => completed = true);

            go.SetActive(true);
            yield return null;

            sequencer.ForceCompleteSequence();

            Assert.IsTrue(completed, "OnSequenceComplete should have been fired synchronously after ForceCompleteSequence.");
        }

        [UnityTest]
        public IEnumerator OnDisable_StopsSequenceAndFiresStopped()
        {
            var actionGo = new GameObject("Action");
            actionGo.transform.SetParent(go.transform);
            var action = actionGo.AddComponent<WaitFramesAction>();
            action.FramesToWait = 100;

            SetField(sequencer, "actions", new List<SequenceAction> { action });

            bool stopped = false;
            sequencer.OnSequenceStopped.AddListener(() => stopped = true);

            go.SetActive(true);
            yield return null;              // 序列開始，action 正在等待

            go.SetActive(false);            // 觸發 OnDisable → StopCurrentSequence

            yield return null;

            Assert.IsTrue(stopped, "OnSequenceStopped should have been fired when GameObject is disabled.");
        }

        [UnityTest]
        public IEnumerator EmptyActions_CompletesImmediately()
        {
            SetField(sequencer, "actions", new List<SequenceAction>());

            bool completed = false;
            sequencer.OnSequenceComplete.AddListener(() => completed = true);

            go.SetActive(true);
            yield return null;
            yield return null;

            Assert.IsTrue(completed, "OnSequenceComplete should have been fired for empty actions list.");
        }
    }
}
