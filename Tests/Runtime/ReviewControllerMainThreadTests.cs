using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Tests
{
    public class ReviewControllerMainThreadTests
    {
        private GameObject _go;
        private ReviewController _controller;
        private ReviewMockConfig _mockConfig;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");
            _mockConfig = ScriptableObject.CreateInstance<ReviewMockConfig>();
            _mockConfig.SimulatedDelaySeconds = 5f;  // long enough that the first call stays in-flight
            _mockConfig.SimulatedError = ReviewErrorCode.NoError;

            _go = new GameObject("[test-controller]");
            _controller = _go.AddComponent<ReviewController>();
            // Force singleton wire-up on main thread before any test dispatches to Task.Run
            var _ = ReviewController.Instance;

            // Inject the mock config via reflection (private [SerializeField])
            var field = typeof(ReviewController).GetField("_mockConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_controller, _mockConfig);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
            _go = null;
            _controller = null;
            if (_mockConfig != null) UnityEngine.Object.DestroyImmediate(_mockConfig);
            _mockConfig = null;
            PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");
        }

        [Test]
        public async Task RequestReview_FromBackgroundThread_ThrowsInvalidOperation()
        {
            var captured = _controller;  // captured on main thread — no Instance access on worker
            var thrown = await Task.Run(() =>
            {
                try { captured.RequestReview(); return (Exception)null; }
                catch (Exception ex) { return ex; }
            });
            Assert.IsInstanceOf<InvalidOperationException>(thrown);
            StringAssert.Contains("main thread", thrown.Message);
        }

        [Test]
        public void RequestReview_SecondCallWhileInFlight_ThrowsInvalidOperation()
        {
            _controller.RequestReview();
            Assert.Throws<InvalidOperationException>(() => _controller.RequestReview());
        }
    }
}
