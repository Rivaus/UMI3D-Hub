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

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using UMI3DHub.Models;
using UMI3DHub.Models.Services;
using UMI3DHub.Services;

namespace UMI3DHub.ViewModels
{
    /// <summary>
    /// Pop upto install a new version of the soft.
    /// </summary>
    public class InstallSoftwareViewModel : ViewModelBase
    {
        #region Fields

        private SoftwareModel softwareCategory;

        private bool displayChooseVersion = true;

        private bool DisplayChooseVersion
        {
            get => displayChooseVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref displayChooseVersion, value);
                if (displayChooseVersion)
                {
                    DisplayDownload = false;
                }
            }
        }

        private bool displayDownload = false;

        private bool DisplayDownload
        {
            get => displayDownload;
            set
            {
                this.RaiseAndSetIfChanged(ref displayDownload, value);
                if (displayDownload)
                {
                    DisplayChooseVersion = false;
                }
            }
        }

        #region ChooseVersionTab

        private ObservableCollection<GithubReleaseModel> releases = new ObservableCollection<GithubReleaseModel>();

        /// <summary>
        /// Stores all releases got of the current soft.
        /// </summary>
        public ObservableCollection<GithubReleaseModel> Releases
        {
            get => releases;
            set {
                this.RaiseAndSetIfChanged(ref releases, value);
                this.RaisePropertyChanged("OfficialReleases");
                this.RaisePropertyChanged("BetaReleases");
            }
        }

        /// <summary>
        /// Selects all official releases from <see cref="Releases"/>.
        /// </summary>
        public IEnumerable<GithubReleaseModel> OfficialReleases
        {
            get => Releases.ToList().Where(r => !r.Prerelease);
        }

        /// <summary>
        /// Selects all beta releases from <see cref="Releases"/>.
        /// </summary>
        public IEnumerable<GithubReleaseModel> BetaReleases
        {
            get => Releases.ToList().Where(r => r.Prerelease);
        }

        private string versionLabel = string.Empty;

        public string VersionLabel { get => versionLabel; set => this.RaiseAndSetIfChanged(ref versionLabel, value); }

        private bool enableChooseVersionBtn = false;

        private bool EnableChooseVersionBtn {
            get => enableChooseVersionBtn;
            set => this.RaiseAndSetIfChanged(ref enableChooseVersionBtn, value);
        }

        private GithubReleaseModel? selectedOfficialVersion = null;

        public GithubReleaseModel? SelectedOfficialVersion
        {
            get => selectedOfficialVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedOfficialVersion, value);
                SelectedVersion = selectedOfficialVersion;
            }
        }

        private GithubReleaseModel? selectedBetaVersion = null;

        public GithubReleaseModel? SelectedBetaVersion
        {
            get => selectedBetaVersion;
            set {
                this.RaiseAndSetIfChanged(ref selectedBetaVersion, value);
                SelectedVersion = selectedBetaVersion;
            }
        }

        /// <summary>
        /// Last version selected from both <see cref="OfficialReleases"/> and <see cref="BetaReleases"/>.
        /// </summary>
        private GithubReleaseModel? selectedVersion = null;

        private GithubReleaseModel? SelectedVersion {
            get => selectedVersion;
            set
            {
                selectedVersion = value;
                EnableChooseVersionBtn = selectedVersion != null;
                VersionLabel = selectedVersion?.Name + " vs " + EnableChooseVersionBtn;
            }
        }

        private ICommand ChooseVersionCommand { get;  }

        #endregion

        #region Downloadtab

        private int downloadProgress = 0;

        public int DownloadProgress
        {
            get => downloadProgress;
            set => this.RaiseAndSetIfChanged(ref downloadProgress, value);
        }

        private string installInformation = string.Empty;

        public string InstallInformation
        {
            get => installInformation;
            set => this.RaiseAndSetIfChanged(ref installInformation, value);
        }


        private string downloadInformation = string.Empty;

        public string DownloadInformation
        {
            get => downloadInformation;
            set => this.RaiseAndSetIfChanged(ref downloadInformation, value);
        }

        private bool visibleSuccessButton = false;

        public bool VisibleSuccessButton { 
            get => visibleSuccessButton;
            set => this.RaiseAndSetIfChanged(ref visibleSuccessButton, value);
        }

        #endregion

        #endregion

        #region Methods

        public InstallSoftwareViewModel(SoftwareModel softwareCategory)
        {
            this.softwareCategory = softwareCategory;

            GetReleases();

            ChooseVersionCommand = ReactiveCommand.Create(ChooseVersion);
        }

        /// <summary>
        /// Pull last releases.
        /// </summary>
        private async void GetReleases()
        {
            VersionLabel = "Getting releases from Github ...";
            EnableChooseVersionBtn = false;

            List<GithubReleaseModel> res = await GitHubService.GetReleasesAsync(softwareCategory.RepoOwner, softwareCategory.RepoName);

            if (res != null)
            {
                if (OfficialReleases.Count() == 0)
                {
                    var official = await GitHubService.GetLastReleaseAsync(softwareCategory.RepoOwner, softwareCategory.RepoName);

                    if (official != null)
                        res.Add(official);
                }

                // Remove already installed version
                foreach(var r in res.ToArray())
                {
                    if (SoftwareManager.Instance.IsVersionInstalled(softwareCategory, r.Id))
                        res.Remove(r);
                }

                Releases = new ObservableCollection<GithubReleaseModel>(res);
                VersionLabel = string.Empty;
            }
            else
            {
                VersionLabel = "No releases found :/ ";
            }
        }

        /// <summary>
        /// Downloads chosen software version.
        /// </summary>
        private void ChooseVersion()
        {
            if (SelectedVersion == null)
                return;

            DisplayDownload = true;

            DownloadProgress = 0;
            DownloadInformation = "Downloading ...";

            GitHubService.DownloadRelease(SelectedVersion, (o, ev) =>
            {
                double bytesIn = double.Parse(ev.BytesReceived.ToString());
                double totalBytes = double.Parse(ev.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                DownloadProgress = (int) percentage;
            },
            (res, id, version, o, ev) =>
            {
                DownloadProgress = 100;
                DownloadInformation = "√ Download Success";
                InstallRelease(res, version, id, SelectedVersion.Prerelease);
            }, 
            () => InstallInformation = "Download failed");
        }

        /// <summary>
        /// Installs downloaded software version.
        /// </summary>
        /// <param name="downloadedFile"></param>
        /// <param name="version"></param>
        /// <param name="githubId"></param>
        /// <param name="isPrerelease"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void InstallRelease(string downloadedFile, string version, int githubId, bool isPrerelease)
        {
            InstallInformation = "Installing ...";

            if (!File.Exists(downloadedFile))
                throw new NotImplementedException();

            try
            {
                string programFiles = SoftwareManager.Instance.InstallationDirectory + "\\" + softwareCategory.RepoName + "\\";

                if (!Directory.Exists(programFiles))
                    Directory.CreateDirectory(programFiles);

                string installFolder = @programFiles + version;

                if (!Directory.Exists(installFolder))
                    Directory.CreateDirectory(installFolder);

                var process = new Process
                {
                    StartInfo =
                    {
                         FileName = downloadedFile,
                         WorkingDirectory = @"C:\",
                         Arguments = "/SILENT /CURRENTUSER /DIR=" + installFolder
                    }
                };

                process.Start();

                var task = process.WaitForExitAsync();

                await task;

                string exePath = string.Empty;

                foreach(var file in Directory.GetFiles(installFolder))
                {
                    if (Path.GetExtension(file) == ".exe" && !file.Contains("unistall"))
                    {
                        exePath = file;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(exePath))
                {
                    InstallInformation = "Error while installing software";
                } else
                {
                    InstallInformation = "\u221A Software successfully installed !";
                    VisibleSuccessButton = true;
                    SoftwareManager.Instance.AddSoftwareVersion(softwareCategory,
                        softwareCategory.Name, installFolder, exePath, version, githubId, isPrerelease);
                }
            }
            catch (Exception ex)
            {
                InstallInformation = "Error while installing file " + ex.Message;
            }

            File.Delete(downloadedFile);
        }

        #endregion
    }
}
