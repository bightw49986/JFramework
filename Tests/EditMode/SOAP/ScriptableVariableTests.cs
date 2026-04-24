using System.Collections.Generic;
using JFramework.SOAP;
using NUnit.Framework;
using UnityEngine;

namespace JFramework.Tests.SOAP
{
    [TestFixture]
    public class ScriptableVariableTests
    {
        private ScriptableInt _scriptableInt;

        [SetUp]
        public void SetUp()
        {
            _scriptableInt = ScriptableObject.CreateInstance<ScriptableInt>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_scriptableInt);
        }

        [Test]
        public void InitialValue_IsDefaultInt()
        {
            Assert.AreEqual(default(int), _scriptableInt.Value);
        }

        [Test]
        public void SetValue_DifferentValue_GetterReturnsNewValue()
        {
            _scriptableInt.Value = 99;
            Assert.AreEqual(99, _scriptableInt.Value);
        }

        [Test]
        public void SetValue_SameValue_ValueChangedNotFired()
        {
            _scriptableInt.Value = 5;
            int callCount = 0;
            _scriptableInt.ValueChanged += (n, o) => callCount++;

            _scriptableInt.Value = 5;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void SetValue_DifferentValue_ValueChangedFiredWithCorrectArgs()
        {
            _scriptableInt.Value = 10;
            int receivedNew = -1;
            int receivedOld = -1;
            _scriptableInt.ValueChanged += (n, o) =>
            {
                receivedNew = n;
                receivedOld = o;
            };

            _scriptableInt.Value = 20;

            Assert.AreEqual(20, receivedNew);
            Assert.AreEqual(10, receivedOld);
        }

        [Test]
        public void SetValue_DifferentValue_AllSubscribersReceiveEvent()
        {
            int countA = 0;
            int countB = 0;
            _scriptableInt.ValueChanged += (n, o) => countA++;
            _scriptableInt.ValueChanged += (n, o) => countB++;

            _scriptableInt.Value = 7;

            Assert.AreEqual(1, countA);
            Assert.AreEqual(1, countB);
        }

        [Test]
        public void ClearValueChangedSubscribers_AfterClear_NoHandlerInvoked()
        {
            int callCount = 0;
            _scriptableInt.ValueChanged += (n, o) => callCount++;
            _scriptableInt.ClearValueChangedSubscribers();

            _scriptableInt.Value = 42;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ImplicitOperator_ReturnsCurrentValue()
        {
            _scriptableInt.Value = 55;
            int x = _scriptableInt;
            Assert.AreEqual(55, x);
        }

        [Test]
        public void SetValue_MultipleChanges_ValueChangedFiredEachTime()
        {
            var receivedValues = new List<int>();
            _scriptableInt.ValueChanged += (n, o) => receivedValues.Add(n);

            _scriptableInt.Value = 1;
            _scriptableInt.Value = 2;
            _scriptableInt.Value = 3;

            Assert.AreEqual(new List<int> { 1, 2, 3 }, receivedValues);
        }
    }
}
