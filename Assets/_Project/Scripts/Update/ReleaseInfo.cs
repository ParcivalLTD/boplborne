using System;

namespace GraceRun.Update
{
    /// <summary>The part of a GitHub "latest release" response the updater reads.</summary>
    [Serializable]
    public class ReleaseInfo
    {
        public string tag_name;
        public ReleaseAsset[] assets;
    }
}
