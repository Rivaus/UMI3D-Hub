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
using System.Diagnostics;
using System.IO;
using UMI3DHub.Models;

namespace UMI3DHub.ViewModels
{
    public class SoftwareManager
    {
        #region Fields

        private static SoftwareManager instance;

        public static SoftwareManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SoftwareManager();

                return instance;
            }
        }

        private Dictionary<int, SoftwareModel> softCategories = new Dictionary<int, SoftwareModel>();

        public delegate void OnSoftwareVersionChangedDelegate(SoftwareModel catergory);
        public event OnSoftwareVersionChangedDelegate OnSoftwareVersionChanged;

        public delegate void OnDirectoryChangedDelegate(string directory);
        public event OnDirectoryChangedDelegate OnInstallationFolderChanged;
        public event OnDirectoryChangedDelegate OnDownloadFolderChanged;

        private string installationDirectory = string.Empty;
        public string InstallationDirectory
        {
            get => installationDirectory;
            set
            {
                if (value != installationDirectory)
                {
                    installationDirectory = value;
                    SaveChanges();
                    OnInstallationFolderChanged?.Invoke(installationDirectory);
                }
            }
        }

        private string downloadDirectory = string.Empty;

        public string DownloadDirectory
        {
            get => downloadDirectory;
            set
            {
                if (downloadDirectory != value)
                {
                    downloadDirectory = value;
                    SaveChanges();
                    OnDownloadFolderChanged?.Invoke(downloadDirectory);
                }
            }
        }

        #endregion

        #region Methods

        private SoftwareManager()
        {
            softCategories.Add(0, new SoftwareModel
            {
                Id = 0,
                RepoOwner = "UMI3D",
                RepoName = "UMI3D-Desktop-Browser",
                Name = "Desktop Browser"
            });

            softCategories.Add(1, new SoftwareModel
            {
                Id = 1,
                RepoOwner = "UMI3D",
                RepoName = "UMI3D-OpenVR-Browser",
                Name = "Open VR"
            });

            installationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UMI3DHUB";
            if (!Directory.Exists(installationDirectory))
                Directory.CreateDirectory(installationDirectory);

            downloadDirectory = Path.GetTempPath();

            LoadData();
        }

        /// <summary>
        /// Gets all software categories handled by the hub.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SoftwareModel> GetSoftwareCategories() => softCategories.Values;

        /// <summary>
        /// Gets a <see cref="SoftwareModel"/> with its <see cref="SoftwareModel.Id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SoftwareModel? GetSoftwareCategoryById(int id)
        {
            if (softCategories.ContainsKey(id))
                return softCategories[id];
            else
                return null;
        }

        /// <summary>
        /// Adds a new <see cref="SoftwareVersionModel"/> to database.
        /// </summary>
        /// <param name="soft"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="exePath"></param>
        /// <param name="version"></param>
        public void AddSoftwareVersion(SoftwareModel soft, string name, string path, string exePath, string version, 
            int githubId, bool isPrerelease)
        {
            int id = 0;
            while (soft.versions.Find(v => v.Id == id) != null)
            {
                id++;
            }

            soft.versions.Insert(0, new SoftwareVersionModel
            {
                Id = id,
                Name = name,
                Path = path,
                Version = version,
                IsFavorite = false,
                ExePath = exePath,
                GithubId = githubId,
                IsPrerelease = isPrerelease
            });

            SaveChanges();

            OnSoftwareVersionChanged?.Invoke(soft);
        }

        /// <summary>
        /// Removes <paramref name="version"/> from database.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="version"></param>
        /// <exception cref="Exception"></exception>
        public void RemoveSoftwareVersion(SoftwareModel category, SoftwareVersionModel version)
        {
            if (category.versions.Contains(version))
            {
                category.versions.Remove(version);

                SaveChanges();

                OnSoftwareVersionChanged?.Invoke(category);
            } else
            {
                throw new Exception();
            }
        }

        public bool IsVersionInstalled(SoftwareModel category, int githubId)
        {
            return category.versions.Find(v => v.GithubId == githubId) != null;
        }

        /// <summary>
        /// Saves current state of the hub.
        /// </summary>
        public async void SaveChanges()
        {
            try
            {
                ApplicationSave saveData = new ApplicationSave
                {
                    InstallationDirectory = InstallationDirectory,
                    DownloadDirectory = DownloadDirectory,
                    Softwares = softCategories.Values
                };

                string save = JsonConvert.SerializeObject(saveData, Formatting.Indented);

                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (!Directory.Exists(path + "/UMI3DHUB"))
                    Directory.CreateDirectory(path + "/UMI3DHUB");

                path += "/UMI3DHUB/data.umi3dhubdata";

                await File.WriteAllTextAsync(path, save);

            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Loads a save file.
        /// </summary>
        public void LoadData()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/UMI3DHUB/data.umi3dhubdata";

            if (File.Exists(path))
            {
                string save = File.ReadAllText(path);

                try
                {
                    ApplicationSave? saveData = JsonConvert.DeserializeObject<ApplicationSave>(save);

                    if (saveData != null)
                    {
                        foreach(var soft in saveData.Softwares)
                        {
                            if (softCategories.ContainsKey(soft.Id))
                            {
                                softCategories[soft.Id].versions = soft.versions;
                                Debug.WriteLine(soft.Name + " -> " + soft.versions.Count);
                            } else
                            {
                                softCategories.Add(soft.Id, soft);
                            }

                            OnSoftwareVersionChanged?.Invoke(soft);
                        }

                        if (!string.IsNullOrEmpty(saveData.InstallationDirectory))
                            InstallationDirectory = saveData.InstallationDirectory;

                        if (!string.IsNullOrEmpty(saveData.DownloadDirectory))
                            DownloadDirectory = saveData.DownloadDirectory;

                        Debug.WriteLine("Installation directory " + InstallationDirectory + " save " + saveData.InstallationDirectory);
                    }
                } catch (Exception ex)
                {
                    Debug.WriteLine("ERROR " + ex.Message);
                }
            }
        }

        #endregion
    }
}
