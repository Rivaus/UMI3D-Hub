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
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;

namespace UMI3DHub.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        public static MainWindowViewModel Instance;

        #region ViewModels

        SoftwaresViewModel softVm;

        public SoftwaresViewModel SoftVm { get => softVm; set => softVm = value; }

        SettingsViewModel settingsVm;

        public SettingsViewModel SettingsVm { get => settingsVm; set => settingsVm = value; }

        public Interaction<InstallSoftwareViewModel, SoftwaresViewModel?> ShowDialog { get; }

        #endregion

        #region Commands

        public ICommand DisplaySettingsCmd { get; }

        public ReactiveCommand<string, Unit> DisplaySoftwareCmd { get; }

        #endregion

        #region Status

        private bool displaySoftwares = true;

        public bool DisplaySoftwares
        {
            get => displaySoftwares;
            set => this.RaiseAndSetIfChanged(ref displaySoftwares, value);
        }

        private bool displaySettings = false;

        public bool DisplaySettings
        {
            get => displaySettings;
            set => this.RaiseAndSetIfChanged(ref displaySettings, value);
        }

        #endregion

        #endregion

        #region Methods

        public MainWindowViewModel()
        {
            if (Instance != null)
                throw new Exception();

            Instance = this;

            ShowDialog = new Interaction<InstallSoftwareViewModel, SoftwaresViewModel?>();

            softVm = new SoftwaresViewModel(this);

            settingsVm = new SettingsViewModel();

            DisplaySettingsCmd = ReactiveCommand.Create(DisplaySettingsPopUp);

            DisplaySoftwareCmd = ReactiveCommand.Create<string>(DisplaySoftware);
        }

        private void DisplaySoftware(string softCategoryId)
        {
            if (int.TryParse(softCategoryId , out int id)) {
                softVm.UpdateContent(id);
                DisplaySoftwares = true;
                DisplaySettings = false;
            }
        }

        private void DisplaySettingsPopUp()
        {
            DisplaySoftwares = false;
            DisplaySettings = true;

            Debug.WriteLine(displaySoftwares);
        }

        #endregion
    }
}
