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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using UMI3DHub.Models;

namespace UMI3DHub.ViewModels
{
    public class SoftwaresViewModel : ViewModelBase
    {
        #region Fields

        private MainWindowViewModel mainViewModel;

        public ICommand InstallSoftwareCommand { get; }

        public SoftwareModel CurrentSoftwareCategory { get; private set; }

        public ObservableCollection<SoftwareVersionModel> CurrentVersions {
            get => currentVersions;
            set => this.RaiseAndSetIfChanged(ref currentVersions, value);
        }

        public ObservableCollection<SoftwareVersionModel> currentVersions = new ObservableCollection<SoftwareVersionModel>();

        public ReactiveCommand<SoftwareVersionModel, Unit> LauchSoftCommand { get; }
        public ReactiveCommand<SoftwareVersionModel, Unit> UnistallSoftCommand { get; }

        #endregion

        #region Methods

        public SoftwaresViewModel(MainWindowViewModel main)
        {
            this.mainViewModel = main;

            this.CurrentSoftwareCategory = SoftwareManager.Instance.GetSoftwareCategoryById(0);

            InstallSoftwareCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var installPopup = new InstallSoftwareViewModel(this.CurrentSoftwareCategory);

                await mainViewModel.ShowDialog.Handle(installPopup);
            });

            LauchSoftCommand = ReactiveCommand.Create<SoftwareVersionModel>(LaunchSoft);

            UnistallSoftCommand = ReactiveCommand.Create<SoftwareVersionModel>(Unistall);

            SoftwareManager.Instance.OnSoftwareVersionChanged += (soft) =>
            {
                if (soft.Id == CurrentSoftwareCategory?.Id)
                    CurrentVersions = new ObservableCollection<SoftwareVersionModel>(soft.versions);

                Debug.WriteLine("On count changed " + CurrentVersions.Count);
            };
            CurrentVersions = new ObservableCollection<SoftwareVersionModel>(CurrentSoftwareCategory?.versions ?? new ());
        }

        /// <summary>
        /// Starts executable associated to <paramref name="soft"/>.
        /// </summary>
        /// <param name="soft"></param>
        /// <exception cref="System.Exception"></exception>
        private void LaunchSoft(SoftwareVersionModel soft)
        {
            if (File.Exists(soft.ExePath))
                Process.Start(soft.ExePath);
            else
                throw new System.Exception();
        }

        /// <summary>
        /// Unistalls a version of a soft.
        /// </summary>
        /// <param name="soft"></param>
        /// <exception cref="System.Exception"></exception>
        private void Unistall(SoftwareVersionModel soft)
        {
            string exePath = string.Empty;

            foreach (var file in Directory.GetFiles(soft.Path))
            {
                if (Path.GetExtension(file) == ".exe" && file.Contains("unins"))
                {
                    exePath = file;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(exePath))
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                         FileName = exePath,
                         WorkingDirectory = @"C:\",
                         Arguments = "/SILENT"
                        }
                    };

                    process.Start();

                    process.WaitForExit();

                    if (Directory.Exists(soft.Path))
                        DeleteDirectory(soft.Path);

                    SoftwareManager.Instance.RemoveSoftwareVersion(CurrentSoftwareCategory, soft);

                } catch (System.Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                    throw new System.Exception();
                }
            }
        }

        /// <summary>
        /// Helper to delete a directory and its content.
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DeleteDirectory(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(dirPath, false);
        }

        public void UpdateContent(int id)
        {
            if (CurrentSoftwareCategory.Id == id)
                return;

            var cat = SoftwareManager.Instance.GetSoftwareCategoryById(id);

            if (cat != null)
            {
                CurrentSoftwareCategory = cat;
                CurrentVersions = new ObservableCollection<SoftwareVersionModel>(CurrentSoftwareCategory.versions);
                this.RaisePropertyChanged("CurrentSoftwareCategory");
                Debug.WriteLine("Change category to " + CurrentSoftwareCategory.Name);
            }
        }

        #endregion
    }
}
