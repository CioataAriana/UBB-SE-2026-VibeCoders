namespace BoardRent.Views
{
    using System;
    using System.ComponentModel;
    using BoardRent.ViewModels;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class AdminPage : Page, INotifyPropertyChanged
    {
        public AdminPage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<AdminViewModel>();

            this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AdminViewModel ViewModel { get; }

        public bool IsUnauthorized => this.ViewModel.IsUnauthorized;

        public Visibility IsAuthorizedVisibility => this.IsUnauthorized ? Visibility.Collapsed : Visibility.Visible;

        public bool IsErrorVisible => this.ViewModel != null && !string.IsNullOrEmpty(this.ViewModel.ErrorMessage);

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(AdminViewModel.ErrorMessage))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsErrorVisible)));
            }
        }

        private async void OnSignOutClicked(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.SignOutCommand.ExecuteAsync(null);
            App.NavigateTo(typeof(LoginPage), true);
        }

        private async void OnResetPasswordClicked(object sender, RoutedEventArgs eventArgs)
        {
            if (this.ViewModel.SelectedUser == null)
            {
                return;
            }

            ContentDialog resetPasswordDialog = new ContentDialog
            {
                Title = $"Reset password for {this.ViewModel.SelectedUser.Username}",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            PasswordBox newPasswordBox = new PasswordBox
            {
                PlaceholderText = "Enter new password"
            };

            resetPasswordDialog.Content = newPasswordBox;

            ContentDialogResult dialogResult = await resetPasswordDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(newPasswordBox.Password))
            {
                await this.ViewModel.ResetPasswordWithValueAsync(newPasswordBox.Password);
            }
        }
    }
}