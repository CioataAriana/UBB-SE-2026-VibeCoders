using BoardRent.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace BoardRent.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = Ioc.Default.GetService<LoginViewModel>();
            this.DataContext = ViewModel;
        }

        private async void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            await ResetPasswordDialog.ShowAsync();
        }
    }
}