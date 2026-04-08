using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BoardRent.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;

namespace BoardRent.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ViewModel { get; }

        public ProfilePage()
        {
            this.InitializeComponent();

            ViewModel = Ioc.Default.GetService<ProfileViewModel>();
            this.DataContext = ViewModel;

            this.Loaded += async (s, e) =>
            {
                await ViewModel.LoadProfile();
                Debug.WriteLine($"Username after load: {ViewModel.Username}");
            };
        }

        private void AdminPanel_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateTo(typeof(AdminPage));
        }
    }
}
