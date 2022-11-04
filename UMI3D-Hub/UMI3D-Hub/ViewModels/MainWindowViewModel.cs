using Avalonia;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using UMI3DHub.Models;

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
