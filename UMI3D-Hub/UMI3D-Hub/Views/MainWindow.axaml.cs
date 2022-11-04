using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using UMI3DHub.ViewModels;

namespace UMI3DHub.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));

            InitTitleBar();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async Task DoShowDialogAsync(InteractionContext<InstallSoftwareViewModel, SoftwaresViewModel?> interaction)
        {
            var dialog = new InstallSoftWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<SoftwaresViewModel?>(this);
            interaction.SetOutput(result);
        }

        private void InitTitleBar()
        {
            this.FindControl<Button>("closeButton").Click += (_, _) => Close();
            this.FindControl<Button>("maximiseButton").Click += (_, _) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };
            this.FindControl<Button>("reduceButton").Click += (_, _) 
                => this.WindowState = WindowState.Minimized; ;
        }
    }
}
