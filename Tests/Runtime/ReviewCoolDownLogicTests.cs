using System;
using NUnit.Framework;
using UnityEngine;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Tests
{
    public class ReviewCoolDownLogicTests
    {
        [SetUp] public void SetUp() => PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");
        [TearDown] public void TearDown() => PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");

        [Test] public void CanRequestReview_ReturnsTrue_WhenNoHistory() {
            var c = new ReviewCoolDownLogic(90);
            Assert.IsTrue(c.CanRequestReview());
            Assert.IsNull(c.LastPromptTimeUtc);
        }

        [Test] public void CanRequestReview_ReturnsFalse_WhenWithinInterval() {
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", DateTime.UtcNow.Ticks.ToString());
            var c = new ReviewCoolDownLogic(90);
            Assert.IsFalse(c.CanRequestReview());
            Assert.Greater(c.Remaining().TotalDays, 89);
        }

        [Test] public void CanRequestReview_ReturnsTrue_WhenIntervalElapsed() {
            var past = DateTime.UtcNow - TimeSpan.FromDays(100);
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", past.Ticks.ToString());
            var c = new ReviewCoolDownLogic(90);
            Assert.IsTrue(c.CanRequestReview());
            Assert.AreEqual(TimeSpan.Zero, c.Remaining());
        }

        [Test] public void StampNow_SetsLastPromptTime() {
            var c = new ReviewCoolDownLogic(90);
            var before = DateTime.UtcNow.AddSeconds(-1);
            c.StampNow();
            Assert.NotNull(c.LastPromptTimeUtc);
            Assert.Greater(c.LastPromptTimeUtc.Value, before);
        }

        [Test] public void ClearForTesting_RemovesStamp() {
            var c = new ReviewCoolDownLogic(90);
            c.StampNow();
            Assert.NotNull(c.LastPromptTimeUtc);
            c.ClearForTesting();
            Assert.IsNull(c.LastPromptTimeUtc);
        }

        [Test] public void LastPromptTimeUtc_ReturnsNull_OnCorruptPlayerPrefs() {
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", "not-a-number");
            var c = new ReviewCoolDownLogic(90);
            Assert.IsNull(c.LastPromptTimeUtc);
            Assert.IsTrue(c.CanRequestReview());
        }

        [Test] public void LastPromptTimeUtc_ReturnsNull_OnEmptyPlayerPrefs() {
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", "");
            var c = new ReviewCoolDownLogic(90);
            Assert.IsNull(c.LastPromptTimeUtc);
        }

        [Test] public void CanRequestReview_ReturnsFalse_WhenLastPromptInFuture() {
            var future = DateTime.UtcNow + TimeSpan.FromDays(30);
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", future.Ticks.ToString());
            var c = new ReviewCoolDownLogic(90);
            Assert.IsFalse(c.CanRequestReview());
        }

        [Test] public void Remaining_ReturnsFullInterval_WhenLastPromptInFuture() {
            var future = DateTime.UtcNow + TimeSpan.FromDays(30);
            PlayerPrefs.SetString("BizSim.Review.LastPromptTicks", future.Ticks.ToString());
            var c = new ReviewCoolDownLogic(90);
            Assert.AreEqual(TimeSpan.FromDays(90), c.Remaining());
        }
    }
}
