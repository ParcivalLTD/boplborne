using GraceRun.Update;
using NUnit.Framework;

namespace GraceRun.Tests.EditMode
{
    public class UpdateRulesTests
    {
        [TestCase("0.1.0", "v0.1.1", true)]
        [TestCase("0.1.9", "v0.1.10", true)]
        [TestCase("0.1.57", "0.1.57", false)]
        [TestCase("0.1.57", "v0.1.2", false)]
        [TestCase("0.1.0", "latest", false)]
        [TestCase("", "v0.1.1", false)]
        public void IsNewer_ComparesNumerically(string current, string latest, bool expected) =>
            Assert.AreEqual(expected, UpdateRules.IsNewer(current, latest));

        [Test]
        public void PickAsset_TakesThePortableZip()
        {
            var release = UpdateRules.ParseRelease(
                "{\"tag_name\":\"v0.1.4\",\"assets\":[" +
                "{\"name\":\"Boplborne_v0.1.4_SingleFile.exe\",\"browser_download_url\":\"https://x/a.exe\",\"size\":5}," +
                "{\"name\":\"Boplborne_v0.1.4_Portable.zip\",\"browser_download_url\":\"https://x/b.zip\",\"size\":9,\"digest\":\"sha256:ABC\"}]}");
            var asset = UpdateRules.PickAsset(release);
            Assert.AreEqual("v0.1.4", release.tag_name);
            Assert.AreEqual("https://x/b.zip", asset.browser_download_url);
            Assert.AreEqual("abc", UpdateRules.ExpectedSha256(asset.digest));
        }

        [Test]
        public void PickAsset_NullWhenNoZip() =>
            Assert.IsNull(UpdateRules.PickAsset(UpdateRules.ParseRelease("{\"tag_name\":\"v1.0.0\",\"assets\":[]}")));

        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{\"message\":\"Not Found\"}")]
        public void ParseRelease_RejectsNonReleases(string json) => Assert.IsNull(UpdateRules.ParseRelease(json));

        [Test]
        public void RuntimeFolder_RoundTrips()
        {
            Assert.IsTrue(UpdateRules.TryParseRuntimeFolder("Runtime_0.1.57", out var version));
            Assert.AreEqual("Runtime_0.1.57", UpdateRules.RuntimeFolder(version));
            Assert.IsFalse(UpdateRules.TryParseRuntimeFolder("Downloads", out _));
        }

        [Test]
        public void IsInside_BlocksZipSlip()
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bop_root");
            Assert.IsTrue(UpdateRules.IsInside(root, System.IO.Path.Combine(root, "data", "a.dll")));
            Assert.IsFalse(UpdateRules.IsInside(root, System.IO.Path.Combine(root, "..", "evil.dll")));
            Assert.IsFalse(UpdateRules.IsInside(root, root + "_sibling" + System.IO.Path.DirectorySeparatorChar + "a.dll"));
        }

        [Test]
        public void OverallProgress_BlendsTheTwoStages()
        {
            Assert.AreEqual(0.8f, UpdateRules.OverallProgress(1f, 0f, 0.8f), 1e-5f);
            Assert.AreEqual(1f, UpdateRules.OverallProgress(1f, 1f, 0.8f), 1e-5f);
            Assert.AreEqual(0.4f, UpdateRules.OverallProgress(0.5f, 0f, 0.8f), 1e-5f);
        }

        [Test]
        public void ProgressText_ShowsMegabytes() =>
            Assert.AreEqual("1.0 / 2.0 MB", UpdateRules.ProgressText(1024 * 1024, 2 * 1024 * 1024));
    }
}
