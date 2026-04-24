using System.Reflection;
using JFramework.SOAP;
using NUnit.Framework;
using UnityEngine;

namespace JFramework.Tests.SOAP
{
    [TestFixture]
    public class ValueReferenceTests
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

        private static void SetScriptableMode(IntReference intRef, ScriptableInt scriptableInt)
        {
            var useScriptableField = typeof(ValueReference<int, ScriptableInt>)
                .GetField("useScriptableValue", BindingFlags.NonPublic | BindingFlags.Instance);
            useScriptableField.SetValue(intRef, true);

            var scriptableField = typeof(ValueReference<int, ScriptableInt>)
                .GetField("scriptableValue", BindingFlags.NonPublic | BindingFlags.Instance);
            scriptableField.SetValue(intRef, scriptableInt);
        }

        // --- Const mode ---

        [Test]
        public void ConstMode_DefaultCtor_ValueIsZero()
        {
            var intRef = new IntReference();
            Assert.AreEqual(0, intRef.Value);
        }

        [Test]
        public void ConstMode_CtorWithValue_ValueIsSet()
        {
            var intRef = new IntReference(42);
            Assert.AreEqual(42, intRef.Value);
        }

        [Test]
        public void ConstMode_SetterChangesValue()
        {
            var intRef = new IntReference(10);
            intRef.Value = 99;
            Assert.AreEqual(99, intRef.Value);
        }

        [Test]
        public void ConstMode_ImplicitOperator_ReturnsValue()
        {
            var intRef = new IntReference(42);
            int x = intRef;
            Assert.AreEqual(42, x);
        }

        [Test]
        public void ConstMode_Equals_TrueWhenMatches()
        {
            var intRef = new IntReference(42);
            Assert.IsTrue(intRef.Equals(42));
        }

        [Test]
        public void ConstMode_Equals_FalseWhenNotMatches()
        {
            var intRef = new IntReference(42);
            Assert.IsFalse(intRef.Equals(99));
        }

        [Test]
        public void ConstMode_ToString_ReturnsValueString()
        {
            var intRef = new IntReference(42);
            Assert.AreEqual("42", intRef.ToString());
        }

        // --- Scriptable mode ---

        [Test]
        public void ScriptableMode_Get_DelegatesToScriptableInt()
        {
            _scriptableInt.Value = 77;
            var intRef = new IntReference();
            SetScriptableMode(intRef, _scriptableInt);

            Assert.AreEqual(77, intRef.Value);
        }

        [Test]
        public void ScriptableMode_Set_ChangesScriptableIntValue()
        {
            var intRef = new IntReference();
            SetScriptableMode(intRef, _scriptableInt);

            intRef.Value = 88;

            Assert.AreEqual(88, _scriptableInt.Value);
        }

        [Test]
        public void ScriptableMode_SubscribeToValueChanged_ReceivesNotification()
        {
            var intRef = new IntReference();
            SetScriptableMode(intRef, _scriptableInt);

            int receivedNew = -1;
            intRef.SubscribeToValueChanged((n, o) => receivedNew = n);

            _scriptableInt.Value = 55;

            Assert.AreEqual(55, receivedNew);
        }
    }
}
