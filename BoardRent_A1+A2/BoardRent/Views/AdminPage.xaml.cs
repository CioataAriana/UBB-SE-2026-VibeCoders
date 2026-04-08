using System;
using System.ComponentModel;
using BoardRent.ViewModels;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BoardRent.Views
{
    public sealed partial class AdminPage : Page, INotifyPropertyChanged
    {
        public AdminViewModel ViewModel { get; }

        public AdminPage()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetService<AdminViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public bool IsUnauthorized => !SessionContext.GetInstance().IsLoggedIn || SessionContext.GetInstance().Role != "Administrator";
        public Visibility IsAuthorizedVisibility => IsUnauthorized ? Visibility.Collapsed : Visibility.Visible;
        public bool IsErrorVisible => ViewModel != null && !string.IsNullOrEmpty(ViewModel.ErrorMessage);

        public event PropertyChangedEventHandler PropertyChanged;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdminViewModel.ErrorMessage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsErrorVisible)));
            }
        }

        private void OnSignOutClicked(object sender, RoutedEventArgs e)
        {
            SessionContext.GetInstance().Clear();
            App.NavigateTo(typeof(LoginPage), clearBackStack: true);
        }

        private async void OnResetPasswordClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUser == null) return;

            var dialog = new ContentDialog
            {
                Title = $"Reset password for {ViewModel.SelectedUser.Username}",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var passwordBox = new PasswordBox { PlaceholderText = "Enter new password" };
            dialog.Content = passwordBox;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                await ViewModel.ResetPasswordWithValueAsync(passwordBox.Password);
            }
        }
    }
}
