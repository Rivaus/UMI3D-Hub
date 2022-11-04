using Avalonia.Controls;

namespace UMI3DHub.Views
{
    public partial class InstallSoftWindow : Window
    {
        public InstallSoftWindow()
        {
            InitializeComponent();

            this.FindControl<Button>("closePopUp").Click += (_, _) => Close();
            this.FindControl<Button>("installSuccess").Click += (_, _) => Close();
        }
    }
}
