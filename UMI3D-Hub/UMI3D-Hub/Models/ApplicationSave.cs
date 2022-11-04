using System.Collections.Generic;

namespace UMI3DHub.Models
{
    public class ApplicationSave
    {
        public string InstallationDirectory { get; set; } = string.Empty;
        public string DownloadDirectory { get; set; } = string.Empty;
        public IEnumerable<SoftwareModel> Softwares { get; set; } = new List<SoftwareModel>();
    }
}
