using System.Collections.Generic;

namespace UMI3DHub.Models
{
    public class SoftwareModel
    {
        public int Id;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepoOwner { get; set; } = string.Empty;
        public string RepoName { get; set; } = string.Empty;

        public List<SoftwareVersionModel> versions = new List<SoftwareVersionModel>();
    }
}
