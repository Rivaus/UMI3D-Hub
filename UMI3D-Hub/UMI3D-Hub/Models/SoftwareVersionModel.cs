using System.Collections.Generic;

namespace UMI3DHub.Models
{
    public class SoftwareVersionModel
    {
        public int Id { get; set; } = 0;
        public int GithubId { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Folder where the soft is installed.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Path of the .exe
        /// </summary>
        public string ExePath { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        public bool IsPrerelease { get; set; } = false;

        public string ReleaseStatus
        {
            get
            {
                if (IsPrerelease)
                    return "Beta";
                else
                    return "Official";
            }
        }

        public bool IsFavorite { get; set; } = false;

        public List<string> Tags { get; set; } = new();
    }
}
