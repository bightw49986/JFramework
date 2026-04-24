using System;
using NUnit.Framework;
using UnityEngine;
using JFramework.SOAP.Event;

namespace JFramework.Tests.StaticEvent
{
    [TestFixture]
    public class StaticEventTests
    {
        private JFramework.SOAP.Event.StaticEvent _ev;

        [SetUp]
        public void SetUp()
        {
            _ev = ScriptableObject.CreateInstance<JFramework.SOAP.Event.StaticEvent>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_ev);
        }

        // Raise 有 listener → listener 被呼叫一次
        [Test]
        public void Raise_WithListener_ListenerCalledOnce()
        {
            int callCount = 0;
            _ev.AddListener((sender, args) => callCount++);

            _ev.Raise(this);

            Assert.AreEqual(1, callCount);
        }

        // Raise 沒有 listener → 不拋出例外
        [Test]
        public void Raise_WithNoListener_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _ev.Raise(this));
        }

        // AddListener 回傳的 handler 就是傳入的 handler（same reference）
        [Test]
        public void AddListener_ReturnsSameHandlerReference()
        {
            EventHandler handler = (sender, args) => { };

            EventHandler returned = _ev.AddListener(handler);

            Assert.AreSame(handler, returned);
        }

        // RemoveListener 後 → 不再收到事件
        [Test]
        public void RemoveListener_AfterRemove_ListenerNotCalled()
        {
            int callCount = 0;
            EventHandler handler = (sender, args) => callCount++;
            _ev.AddListener(handler);
            _ev.RemoveListener(handler);

            _ev.Raise(this);

            Assert.AreEqual(0, callCount);
        }

        // 多個 listener 都收到事件
        [Test]
        public void Raise_MultipleListeners_AllListenersCalled()
        {
            int callCount = 0;
            _ev.AddListener((sender, args) => callCount++);
            _ev.AddListener((sender, args) => callCount++);
            _ev.AddListener((sender, args) => callCount++);

            _ev.Raise(this);

            Assert.AreEqual(3, callCount);
        }

        // Raise(sender) → sender 正確傳遞
        [Test]
        public void Raise_WithSender_SenderCorrectlyPassed()
        {
            object receivedSender = null;
            _ev.AddListener((sender, args) => receivedSender = sender);

            _ev.Raise(this);

            Assert.AreSame(this, receivedSender);
        }

        // Raise(sender, customArgs) → EventArgs 正確傳遞
        [Test]
        public void Raise_WithCustomArgs_EventArgsCorrectlyPassed()
        {
            EventArgs customArgs = new EventArgs();
            EventArgs receivedArgs = null;
            _ev.AddListener((sender, args) => receivedArgs = args);

            _ev.Raise(this, customArgs);

            Assert.AreSame(customArgs, receivedArgs);
        }
    }

    [TestFixture]
    public class StaticEventGenericTests
    {
        private EventOneArg_Int _ev;

        [SetUp]
        public void SetUp()
        {
            _ev = ScriptableObject.CreateInstance<EventOneArg_Int>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_ev);
        }

        // Raise(sender, int) → listener 收到正確的 int 值
        [Test]
        public void Raise_WithIntArg_ListenerReceivesCorrectValue()
        {
            int receivedValue = 0;
            _ev.AddListener((sender, value) => receivedValue = value);

            _ev.Raise(this, 42);

            Assert.AreEqual(42, receivedValue);
        }

        // RemoveListener 回傳的 handler 就是傳入的 handler（same reference）
        [Test]
        public void RemoveListener_ReturnsSameHandlerReference()
        {
            EventHandler<int> handler = (sender, value) => { };

            EventHandler<int> returned = _ev.RemoveListener(handler);

            Assert.AreSame(handler, returned);
        }

        // 多個 listener 都收到 int 值
        [Test]
        public void Raise_MultipleListeners_AllReceiveCorrectIntValue()
        {
            int sum = 0;
            _ev.AddListener((sender, value) => sum += value);
            _ev.AddListener((sender, value) => sum += value);
            _ev.AddListener((sender, value) => sum += value);

            _ev.Raise(this, 10);

            Assert.AreEqual(30, sum);
        }
    }
}
