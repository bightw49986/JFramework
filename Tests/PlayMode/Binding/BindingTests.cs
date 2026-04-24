using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using JFramework.Binding;
using JFramework.SOAP;

namespace JFramework.Tests.PlayMode.Binding
{
    /// <summary>
    /// 走 inheritance 往上找欄位，同時涵蓋 private（子類）和 protected（基類）
    /// </summary>
    internal static class ReflectionHelper
    {
        public static void SetField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (field != null) { field.SetValue(obj, value); return; }
                type = type.BaseType;
            }
            throw new System.Exception($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // FillAmountBinder
    // ══════════════════════════════════════════════════════════════════════════

    [TestFixture]
    public class FillAmountBinderTests
    {
        private GameObject go;
        private Image image;
        private FillAmountBinder binder;
        private ScriptableFloat fillSource;

        [SetUp]
        public void SetUp()
        {
            fillSource = ScriptableObject.CreateInstance<ScriptableFloat>();

            // 先停用，設完欄位再啟用，確保 OnEnable 拿到正確的 reference
            go = new GameObject("FillBinder");
            go.SetActive(false);

            image = go.AddComponent<Image>();
            binder = go.AddComponent<FillAmountBinder>();

            ReflectionHelper.SetField(binder, "image", image);
            ReflectionHelper.SetField(binder, "fillAmount", fillSource);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(fillSource);
        }

        // ─── OnValueChanged 模式 ────────────────────────────────────────────

        [UnityTest]
        public IEnumerator OnValueChanged_ScriptableFloatChanges_UpdatesFillAmount()
        {
            ReflectionHelper.SetField(binder, "bindingType", BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            fillSource.Value = 0.75f;

            Assert.AreEqual(0.75f, image.fillAmount, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnValueChanged_ValueAboveOne_ClampedToOne()
        {
            ReflectionHelper.SetField(binder, "bindingType", BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            fillSource.Value = 2.5f;

            Assert.AreEqual(1f, image.fillAmount, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnValueChanged_ValueBelowZero_ClampedToZero()
        {
            ReflectionHelper.SetField(binder, "bindingType", BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            fillSource.Value = -1f;

            Assert.AreEqual(0f, image.fillAmount, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnDisable_Unsubscribes_ValueChangeNoLongerUpdatesImage()
        {
            ReflectionHelper.SetField(binder, "bindingType", BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            go.SetActive(false); // 觸發 OnDisable，取消訂閱
            fillSource.Value = 0.9f;

            Assert.AreNotEqual(0.9f, image.fillAmount);
        }

        // ─── Update 模式 ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Update_ScriptableFloatChanges_UpdatesFillAmountNextFrame()
        {
            ReflectionHelper.SetField(binder, "bindingType", BindingType.Update);
            go.SetActive(true);
            yield return null; // 等第一幀 Update 跑完

            fillSource.Value = 0.6f;
            yield return null; // 等 Update 偵測到變化

            Assert.AreEqual(0.6f, image.fillAmount, 0.001f);
        }

        // ─── setFillAtEnable ────────────────────────────────────────────────

        [Test]
        public void SetFillAtEnable_True_SetsFillAmountImmediatelyOnEnable()
        {
            fillSource.Value = 0.4f;
            ReflectionHelper.SetField(binder, "setFillAtEnable", true);
            ReflectionHelper.SetField(binder, "bindingType", BindingType.OnValueChanged);

            go.SetActive(true); // OnEnable 同步執行

            Assert.AreEqual(0.4f, image.fillAmount, 0.001f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TextBinder
    // ══════════════════════════════════════════════════════════════════════════

    [TestFixture]
    public class TextBinderTests
    {
        private GameObject go;
        private Text textComponent;
        private TextBinder binder;
        private ScriptableString stringSource;
        private ScriptableInt intSource;

        [SetUp]
        public void SetUp()
        {
            stringSource = ScriptableObject.CreateInstance<ScriptableString>();
            intSource    = ScriptableObject.CreateInstance<ScriptableInt>();

            go = new GameObject("TextBinder");
            go.SetActive(false);

            textComponent = go.AddComponent<Text>();
            binder = go.AddComponent<TextBinder>();

            ReflectionHelper.SetField(binder, "targetText",   textComponent);
            ReflectionHelper.SetField(binder, "stringSource", stringSource);
            ReflectionHelper.SetField(binder, "intSource",    intSource);
            ReflectionHelper.SetField(binder, "textFormat",   "{0}");
            ReflectionHelper.SetField(binder, "setTextAtEnable", false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(stringSource);
            Object.DestroyImmediate(intSource);
        }

        // ─── OnValueChanged + String ────────────────────────────────────────

        [UnityTest]
        public IEnumerator OnValueChanged_StringSource_ChangingValueUpdatesText()
        {
            ReflectionHelper.SetField(binder, "bindingSource", BindingSource.String);
            ReflectionHelper.SetField(binder, "bindingType",   BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            stringSource.Value = "Hello";

            Assert.AreEqual("Hello", textComponent.text);
        }

        // ─── OnValueChanged + Int ───────────────────────────────────────────

        [UnityTest]
        public IEnumerator OnValueChanged_IntSource_ChangingValueUpdatesText()
        {
            ReflectionHelper.SetField(binder, "bindingSource", BindingSource.Int);
            ReflectionHelper.SetField(binder, "bindingType",   BindingType.OnValueChanged);
            go.SetActive(true);
            yield return null;

            intSource.Value = 42;

            Assert.AreEqual("42", textComponent.text);
        }

        // ─── Update 模式 ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Update_StringSource_ChangingValueUpdatesTextNextFrame()
        {
            ReflectionHelper.SetField(binder, "bindingSource", BindingSource.String);
            ReflectionHelper.SetField(binder, "bindingType",   BindingType.Update);
            go.SetActive(true);
            yield return null;

            stringSource.Value = "Updated";
            yield return null;

            Assert.AreEqual("Updated", textComponent.text);
        }

        // ─── textFormat prefix / suffix ─────────────────────────────────────

        [UnityTest]
        public IEnumerator OnValueChanged_WithTextFormat_AppliesPrefixAndSuffix()
        {
            ReflectionHelper.SetField(binder, "bindingSource", BindingSource.String);
            ReflectionHelper.SetField(binder, "bindingType",   BindingType.OnValueChanged);
            ReflectionHelper.SetField(binder, "textFormat",    "HP: {0} pts");
            go.SetActive(true);
            yield return null;

            stringSource.Value = "100";

            Assert.AreEqual("HP: 100 pts", textComponent.text);
        }

        // ─── setTextAtEnable ────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator SetTextAtEnable_True_SetsTextImmediatelyOnEnable()
        {
            stringSource.Value = "Ready";
            ReflectionHelper.SetField(binder, "bindingSource",   BindingSource.String);
            ReflectionHelper.SetField(binder, "bindingType",     BindingType.Update);
            ReflectionHelper.SetField(binder, "setTextAtEnable", true);
            go.SetActive(true);

            // OnEnable 呼叫 SetTextWithCurrentValue，不需要等 Update
            yield return null;

            Assert.AreEqual("Ready", textComponent.text);
        }
    }
}
