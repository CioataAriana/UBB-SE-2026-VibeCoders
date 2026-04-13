using System.Diagnostics;
using BoardRent.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BoardRent.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<ProfileViewModel>();
            this.DataContext = this.ViewModel;

            this.ViewModel.Navigate = (type) =>
            {
                App.NavigateTo(type, true);
            };

            this.Loaded += async (s, e) =>
            {
                await this.ViewModel.LoadProfile();
            };
        }
        public ProfileViewModel ViewModel { get; }
    }
}