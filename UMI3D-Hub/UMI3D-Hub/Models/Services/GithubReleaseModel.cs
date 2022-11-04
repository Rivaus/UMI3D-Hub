
using System.Collections.Generic;
using UMI3DHub.Services;

namespace UMI3DHub.Models.Services
{
    public class GithubReleaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool Prerelease { get; set; } = false;

        public List<GithubAssetModel> Assets { get; set; } = new List<GithubAssetModel>();
    }
}
