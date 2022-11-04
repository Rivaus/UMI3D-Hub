
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UMI3DHub.Models.Services;
using UMI3DHub.ViewModels;

namespace UMI3DHub.Services
{
    internal class GitHubService
    {
        #region Fields

        static HttpClient client = new HttpClient();
        public const string apiUrl = "https://api.github.com/repos/";
        public const string releasesEndPoint = "/releases";
        public const string lastReleasesEndPoint = "/releases/latest";

        #endregion

        #region Methods

        public static async Task<List<GithubReleaseModel>> GetReleasesAsync(string repositoryOwner, string repositoryName)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            string path = apiUrl + repositoryOwner + "/" + repositoryName + releasesEndPoint;
            List<GithubReleaseModel> releases = new List<GithubReleaseModel>();

            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                string dataStr = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<GithubReleaseModel>>(dataStr);

                if (data != null)
                    releases = data;
            }

            return releases;
        }

        public static async Task<GithubReleaseModel> GetLastReleaseAsync(string repositoryOwner, string repositoryName)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            string path = apiUrl + repositoryOwner + "/" + repositoryName + lastReleasesEndPoint;
            GithubReleaseModel release = new GithubReleaseModel();

            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                string dataStr = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GithubReleaseModel>(dataStr);

                if (data != null)
                    release = data;
            }

            return release;
        }

        public static void DownloadRelease(GithubReleaseModel release, 
            Action<object, DownloadProgressChangedEventArgs> onProgressChanged,
            Action<string, int, string, object?, AsyncCompletedEventArgs> onDownloadFinish,
            Action onDownloadFailed)
        {
            var exe = FindReleaseExecutable(release);

            if (exe == null)
            {
                onDownloadFailed?.Invoke();
                return;
            }

            var res =  SoftwareManager.Instance.DownloadDirectory + "/umi3d-hub-" + Guid.NewGuid() + ".exe";

            WebClient client = new WebClient();
            client.DownloadFileAsync(new Uri(exe.Browser_download_url), res);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(onProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, ev) =>
            {
                onDownloadFinish?.Invoke(res, release.Id, release.Name, sender, ev);
                client.Dispose();
            });
        }

        private static GithubAssetModel? FindReleaseExecutable(GithubReleaseModel release)
        {
            if (release == null)
                return null;

            return release.Assets.Find(a => Path.GetExtension(a?.Browser_download_url ?? string.Empty) == ".exe");
        }

        #endregion
    }
}
