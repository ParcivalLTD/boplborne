using System;

namespace GraceRun.Update
{
    /// <summary>One file attached to a GitHub release (field names match the GitHub API JSON).</summary>
    [Serializable]
    public class ReleaseAsset
    {
        public string name;
        public string browser_download_url;
        public long size;
        /// <summary>"sha256:&lt;hex&gt;" when GitHub provides it, otherwise empty.</summary>
        public string digest;
    }
}
