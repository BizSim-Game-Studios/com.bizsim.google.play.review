using NUnit.Framework;
using UnityEngine;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class DiagnosticSnapshotTests
    {
        [Test]
        public void SchemaVersion_IsOne()
        {
            var snap = new ReviewDiagnosticSnapshot();
            Assert.AreEqual(1, snap.SchemaVersion);
        }

        [Test]
        public void JsonRoundTrip_PreservesFields()
        {
            var snap = new ReviewDiagnosticSnapshot
            {
                SessionCount = 10,
                CooldownActive = true,
                RemoteEnabled = true,
                ConsentGranted = false,
                LastErrorCode = "TIMEOUT"
            };
            var json = JsonUtility.ToJson(snap);
            var restored = JsonUtility.FromJson<ReviewDiagnosticSnapshot>(json);

            Assert.AreEqual(10, restored.SessionCount);
            Assert.IsTrue(restored.CooldownActive);
            Assert.AreEqual("TIMEOUT", restored.LastErrorCode);
        }
    }
}
