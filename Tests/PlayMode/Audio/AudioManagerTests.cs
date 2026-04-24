using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using JFramework.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JFramework.Tests.PlayMode.Audio
{
    public class AudioManagerTests
    {
        private AudioManager mgr;

        [SetUp]
        public void SetUp()
        {
            mgr = new AudioManager();
        }

        [TearDown]
        public void TearDown()
        {
            mgr?.Dispose();
            mgr = null;
        }

        private void RegisterClip(AudioManager manager, string audioID, AudioClip clip)
        {
            var field = typeof(AudioManager).GetField("audioClips",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var dict = (Dictionary<string, AudioClip>)field.GetValue(manager);
            dict[audioID] = clip;
        }

        [UnityTest]
        public IEnumerator Constructor_CreatesAudioSourceContainer()
        {
            yield return null;

            var container = GameObject.Find("AudioSourceContainer");
            Assert.IsNotNull(container);
        }

        [UnityTest]
        public IEnumerator PlayAudio_UnknownAudioID_ReturnsNullAndLogsWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex(".*"));
            var player = mgr.PlayAudio("nonexistent", normalizedTime: 0f);

            Assert.IsNull(player);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayAudio_KnownAudioID_ReturnsNonNullPlayer()
        {
            var clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            RegisterClip(mgr, "test_clip", clip);

            var player = mgr.PlayAudio("test_clip", normalizedTime: 0f);

            Assert.IsNotNull(player);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayAudio_KnownAudioID_PlayerHasCorrectAudioID()
        {
            var clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            RegisterClip(mgr, "test_clip", clip);

            var player = mgr.PlayAudio("test_clip", normalizedTime: 0f);

            Assert.AreEqual("test_clip", player.AudioID);

            yield return null;
        }

        [UnityTest]
        public IEnumerator StopAudio_ActivePlayer_StopsPlayback()
        {
            var clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            RegisterClip(mgr, "test_clip", clip);

            var player = mgr.PlayAudio("test_clip", normalizedTime: 0f);
            mgr.StopAudio(player);

            Assert.IsFalse(player.AudioSource.isPlaying);

            yield return null;
        }

        [Test]
        public void Dispose_DestroysAudioSourceContainer()
        {
            mgr.Dispose();
            mgr = null;

            var container = GameObject.Find("AudioSourceContainer");
            Assert.IsNull(container);
        }
    }
}
