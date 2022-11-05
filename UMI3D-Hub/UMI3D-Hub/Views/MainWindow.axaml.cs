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
