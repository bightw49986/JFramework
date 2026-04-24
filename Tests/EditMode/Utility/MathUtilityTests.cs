using NUnit.Framework;
using UnityEngine;
using JFramework.Utility;

namespace JFramework.Tests.Utility
{
    [TestFixture]
    public class MathUtilityTests
    {
        // --- SineWave ---

        [Test]
        public void SineWave_FrequencyOne_TimeZero_ReturnsZero()
        {
            float result = MathUtility.SineWave(1f, 0f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void SineWave_FrequencyOne_QuarterPeriod_ReturnsOne()
        {
            float result = MathUtility.SineWave(1f, 0.25f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        // --- CosineWave ---

        [Test]
        public void CosineWave_FrequencyOne_TimeZero_ReturnsOne()
        {
            float result = MathUtility.CosineWave(1f, 0f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        [Test]
        public void CosineWave_FrequencyOne_HalfPeriod_ReturnsNegativeOne()
        {
            float result = MathUtility.CosineWave(1f, 0.5f);
            Assert.AreEqual(-1f, result, 0.0001f);
        }

        // --- SineWave01 / SineWave10 ---

        [Test]
        public void SineWave01_FrequencyOne_TimeZero_ReturnsZero()
        {
            float result = MathUtility.SineWave01(1f, 0f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void SineWave10_FrequencyOne_TimeZero_ReturnsOne()
        {
            float result = MathUtility.SineWave10(1f, 0f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        [Test]
        public void SineWave01AndSineWave10_SumIsOne_AtArbitraryTime()
        {
            float t = 0.37f;
            float sum = MathUtility.SineWave01(1f, t) + MathUtility.SineWave10(1f, t);
            Assert.AreEqual(1f, sum, 0.0001f);
        }

        // --- PingPongWave ---

        [Test]
        public void PingPongWave_FrequencyOne_TimeZero_ReturnsZero()
        {
            float result = MathUtility.PingPongWave(1f, 0f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void PingPongWave_FrequencyOne_HalfPeriod_ReturnsOne()
        {
            float result = MathUtility.PingPongWave(1f, 0.5f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        // --- SawtoothWave ---

        [Test]
        public void SawtoothWave_FrequencyOne_TimeZero_ReturnsZero()
        {
            float result = MathUtility.SawtoothWave(1f, 0f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void SawtoothWave_FrequencyOne_HalfPeriod_ReturnsHalf()
        {
            float result = MathUtility.SawtoothWave(1f, 0.5f);
            Assert.AreEqual(0.5f, result, 0.0001f);
        }

        // --- RemapMinusOneOneTo01 ---

        [Test]
        public void RemapMinusOneOneTo01_NegativeOne_ReturnsZero()
        {
            float result = MathUtility.RemapMinusOneOneTo01(-1f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void RemapMinusOneOneTo01_Zero_ReturnsHalf()
        {
            float result = MathUtility.RemapMinusOneOneTo01(0f);
            Assert.AreEqual(0.5f, result, 0.0001f);
        }

        [Test]
        public void RemapMinusOneOneTo01_PositiveOne_ReturnsOne()
        {
            float result = MathUtility.RemapMinusOneOneTo01(1f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        // --- Remap01ToMinusOneOne ---

        [Test]
        public void Remap01ToMinusOneOne_Zero_ReturnsNegativeOne()
        {
            float result = MathUtility.Remap01ToMinusOneOne(0f);
            Assert.AreEqual(-1f, result, 0.0001f);
        }

        [Test]
        public void Remap01ToMinusOneOne_Half_ReturnsZero()
        {
            float result = MathUtility.Remap01ToMinusOneOne(0.5f);
            Assert.AreEqual(0f, result, 0.0001f);
        }

        [Test]
        public void Remap01ToMinusOneOne_One_ReturnsPositiveOne()
        {
            float result = MathUtility.Remap01ToMinusOneOne(1f);
            Assert.AreEqual(1f, result, 0.0001f);
        }

        // --- EvaluateSquarePerimeter ---

        [Test]
        public void EvaluateSquarePerimeter_T0_ReturnsBottomLeft()
        {
            Vector2 result = MathUtility.EvaluateSquarePerimeter(0f);
            Assert.That(result.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void EvaluateSquarePerimeter_T025_ReturnsBottomRight()
        {
            Vector2 result = MathUtility.EvaluateSquarePerimeter(0.25f);
            Assert.That(result.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void EvaluateSquarePerimeter_T05_ReturnsTopRight()
        {
            Vector2 result = MathUtility.EvaluateSquarePerimeter(0.5f);
            Assert.That(result.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void EvaluateSquarePerimeter_T075_ReturnsTopLeft()
        {
            Vector2 result = MathUtility.EvaluateSquarePerimeter(0.75f);
            Assert.That(result.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void EvaluateSquarePerimeter_Clockwise_T025_ReturnsTopLeft()
        {
            Vector2 result = MathUtility.EvaluateSquarePerimeter(0.25f, MathUtility.RectCorner.BottomLeft, clockwise: true);
            Assert.That(result.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1f).Within(0.0001f));
        }

        // --- EvaluateRectPerimeter ---

        [Test]
        public void EvaluateRectPerimeter_CenteredOrigin_Size2x2_T0_ReturnsMinusOneMinusOne()
        {
            Vector2 result = MathUtility.EvaluateRectPerimeter(Vector2.zero, new Vector2(2f, 2f), 0f);
            Assert.That(result.x, Is.EqualTo(-1f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(-1f).Within(0.0001f));
        }

        [Test]
        public void EvaluateRectPerimeter_CenteredOneOne_Size2x2_T0_ReturnsZeroZero()
        {
            Vector2 result = MathUtility.EvaluateRectPerimeter(new Vector2(1f, 1f), new Vector2(2f, 2f), 0f);
            Assert.That(result.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
        }
    }
}
