
/*
Copyright 2022 Quentin Tran
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

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
    public class GitHubService
    {
        #region Fields

        static HttpClient client = new HttpClient();

        public const string apiUrl = "https://api.github.com/repos/";

        public const string releasesEndPoint = "/releases";
        public const string lastReleasesEndPoint = "/releases/latest";

        #endregion

        #region Methods

        /// <summary>
        /// Gets last releases of a repository.
        /// </summary>
        /// <param name="repositoryOwner"></param>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets latest release of a repository.
        /// </summary>
        /// <param name="repositoryOwner"></param>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Download an exe associated to a Github release.
        /// </summary>
        /// <param name="release"></param>
        /// <param name="onProgressChanged"></param>
        /// <param name="onDownloadFinish"></param>
        /// <param name="onDownloadFailed"></param>
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

        /// <summary>
        /// Finds an exe file among Github release's assets.
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        private static GithubAssetModel? FindReleaseExecutable(GithubReleaseModel release)
        {
            if (release == null)
                return null;

            return release.Assets.Find(a => Path.GetExtension(a?.Browser_download_url ?? string.Empty) == ".exe");
        }

        #endregion
    }
}
