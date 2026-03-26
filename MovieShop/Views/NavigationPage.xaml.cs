using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;
using MovieShop.Models;

namespace MovieShop.Views
{
    public sealed partial class NavigationPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Load default page on startup
            NavigateToCurrentView();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentViewModel))
            {
                NavigateToCurrentView();
            }
        }

        private void NavigateToCurrentView()
        {
            var current = ViewModel.CurrentViewModel as string;

            if (ViewModel.CurrentViewModel is WalletViewModel walletVM)
            {
                ContentArea.Content = new WalletView(walletVM);
            }
            else if (current == "Marketplace")
            {
                ContentArea.Content = new MovieShop.Views.StartPageEquipment();
            }
            else if (current == "Shop")
            {
                ContentArea.Content = new MainPage();
            }
            else if (current == "SalesPage")
            {
                ContentArea.Content = new MovieShop.Views.MovieShopView
                {
                    HostViewModel = ViewModel,
                    ShowOnlySales = true
                };
            }
            else if (current == "FullShop")
            {
                ContentArea.Content = new MovieShop.Views.MovieShopView
                {
                    HostViewModel = ViewModel,
                    ShowOnlySales = false
                };
            }
        }

        public void NavigateToMovieDetail(Movie movie, bool showOnlySales)
        {
            ContentArea.Content = new MovieShop.Views.MovieShopView
            {
                HostViewModel = ViewModel,
                ShowOnlySales = showOnlySales,
                InitialMovie = movie
            };
        }
    }
}