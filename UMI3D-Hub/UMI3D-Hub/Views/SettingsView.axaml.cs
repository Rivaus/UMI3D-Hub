using Avalonia.Controls;
using Avalonia.VisualTree;
using System.Diagnostics;
using UMI3DHub.ViewModels;

namespace UMI3DHub.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            Button setInstallationDirBtn = this.FindControl<Button>("setInstallationDirectoryButton");
            setInstallationDirBtn.Content = SoftwareManager.Instance.InstallationDirectory;

            setInstallationDirBtn.Click += async (_, _) =>
            {
                var dialogue = new OpenFolderDialog();
                dialogue.Directory = SoftwareManager.Instance.InstallationDirectory;

                var window = this.Parent?.FindAncestorOfType<Window>();

                if (window != null)
                {
                    var res = dialogue.ShowAsync(window);

                    await res;

                    if (res.Result != SoftwareManager.Instance.InstallationDirectory && !string.IsNullOrEmpty(res.Result))
                        SoftwareManager.Instance.InstallationDirectory = res.Result;
                }
            };

            SoftwareManager.Instance.OnInstallationFolderChanged += dir =>
            {
                setInstallationDirBtn.Content = dir;
            };

            Button setDownloadDirBtn = this.FindControl<Button>("setDownloadDirectoryButton");
            setDownloadDirBtn.Content = SoftwareManager.Instance.DownloadDirectory;
            setDownloadDirBtn.Click += async (_, _) =>
            {
                var dialogue = new OpenFolderDialog();
                dialogue.Directory = SoftwareManager.Instance.DownloadDirectory;

                var window = this.Parent?.FindAncestorOfType<Window>();

                if (window != null)
                {
                    var res = dialogue.ShowAsync(window);

                    await res;

                    if (res.Result != SoftwareManager.Instance.DownloadDirectory && !string.IsNullOrEmpty(res.Result))
                        SoftwareManager.Instance.DownloadDirectory = res.Result;
                }
            };

            SoftwareManager.Instance.OnDownloadFolderChanged += dir =>
            {
                setDownloadDirBtn.Content = dir;
            };
        }
    }
}
