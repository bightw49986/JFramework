using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using JFramework.SceneManagement;
using JFramework.SOAP;
using JFramework.SOAP.Event;

namespace JFramework.Tests.PlayMode.SceneManagement
{
    public class SceneManagementTests
    {
        private ChangeSceneEvent _changeSceneEvent;
        private SceneModule _sceneModule;
        private GameObject _objectRoot;

        [TearDown]
        public void TearDown()
        {
            if (_sceneModule != null)
            {
                _sceneModule.Terminate();
                UnityEngine.Object.DestroyImmediate(_sceneModule);
                _sceneModule = null;
            }

            if (_changeSceneEvent != null)
            {
                UnityEngine.Object.DestroyImmediate(_changeSceneEvent);
                _changeSceneEvent = null;
            }

            if (_objectRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(_objectRoot);
                _objectRoot = null;
            }
        }

        // --- Helper: 走繼承鏈取 FieldInfo ---

        static FieldInfo FindField(Type type, string name)
        {
            while (type != null && type != typeof(object))
            {
                var fi = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fi != null) return fi;
                type = type.BaseType;
            }
            return null;
        }

        static void SetField(object obj, string name, object value)
        {
            var fi = FindField(obj.GetType(), name);
            Assert.IsNotNull(fi, $"Field '{name}' not found on {obj.GetType().FullName}");
            fi.SetValue(obj, value);
        }

        static T GetField<T>(object obj, string name)
        {
            var fi = FindField(obj.GetType(), name);
            Assert.IsNotNull(fi, $"Field '{name}' not found on {obj.GetType().FullName}");
            return (T)fi.GetValue(obj);
        }

        // =========================================================
        // Test 1: ChangeSceneEvent AddListener 能收到 Raised 事件
        // =========================================================
        [Test]
        public void ChangeSceneEvent_AddListener_ReceivesRaisedEvent()
        {
            _changeSceneEvent = ScriptableObject.CreateInstance<ChangeSceneEvent>();

            bool received = false;
            EventHandler<ChangeSceneInfo> handler = (sender, args) => received = true;
            _changeSceneEvent.AddListener(handler);

            var info = new ChangeSceneInfo { SceneName = new StringReference("TestScene") };
            _changeSceneEvent.Raise(this, info);

            Assert.IsTrue(received, "Listener should have been called after Raise.");
        }

        // =========================================================
        // Test 2: RemoveListener 後不再收到事件
        // =========================================================
        [Test]
        public void ChangeSceneEvent_RemoveListener_NoLongerReceivesEvent()
        {
            _changeSceneEvent = ScriptableObject.CreateInstance<ChangeSceneEvent>();

            bool received = false;
            EventHandler<ChangeSceneInfo> handler = (sender, args) => received = true;
            _changeSceneEvent.AddListener(handler);
            _changeSceneEvent.RemoveListener(handler);

            var info = new ChangeSceneInfo { SceneName = new StringReference("TestScene") };
            _changeSceneEvent.Raise(this, info);

            Assert.IsFalse(received, "Listener should NOT be called after RemoveListener.");
        }

        // =========================================================
        // Test 3: ChangeSceneInfo.SceneName 可透過 StringReference 設定
        // =========================================================
        [Test]
        public void ChangeSceneInfo_SceneName_CanBeSetViaStringReference()
        {
            var info = new ChangeSceneInfo
            {
                SceneName = new StringReference("TestScene")
            };

            Assert.AreEqual("TestScene", info.SceneName.Value);
        }

        // =========================================================
        // Test 4: Initialize 後 isChangingScene 應為 false
        // =========================================================
        [UnityTest]
        public IEnumerator SceneModule_Initialize_ListensToChangeSceneEvent()
        {
            _changeSceneEvent = ScriptableObject.CreateInstance<ChangeSceneEvent>();
            _sceneModule = ScriptableObject.CreateInstance<SceneModule>();

            SetField(_sceneModule, "changeSceneEvent", _changeSceneEvent);

            _objectRoot = new GameObject("TestRoot");
            _sceneModule.Initialize(_objectRoot);

            yield return null;

            bool isChangingScene = GetField<bool>(_sceneModule, "isChangingScene");
            Assert.IsFalse(isChangingScene, "isChangingScene should be false after Initialize.");
        }

        // =========================================================
        // Test 5: 當 isChangingScene == true 時，Raise 會觸發 LogError
        // =========================================================
        [UnityTest]
        public IEnumerator SceneModule_WhenAlreadyChangingScene_LogsError()
        {
            _changeSceneEvent = ScriptableObject.CreateInstance<ChangeSceneEvent>();
            _sceneModule = ScriptableObject.CreateInstance<SceneModule>();

            SetField(_sceneModule, "changeSceneEvent", _changeSceneEvent);

            _objectRoot = new GameObject("TestRoot");
            _sceneModule.Initialize(_objectRoot);

            SetField(_sceneModule, "isChangingScene", true);

            LogAssert.Expect(LogType.Error, new Regex("already in progress"));

            var info = new ChangeSceneInfo { SceneName = new StringReference("SomeScene") };
            _changeSceneEvent.Raise(this, info);

            yield return null;
        }
    }
}
