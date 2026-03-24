using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;

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
            else if (current == "Shop")
            {
                // Shows the Red Banner
                ContentArea.Content = new MovieShop.Views.MainPage();
            }
            else if (current == "SalesPage")
            {
                ContentArea.Content = new MovieShop.Views.MovieShopView { ShowOnlySales = true };
            }
            else if (current == "FullShop")
            {
                // This is for your Navbar "Shop" button
                ContentArea.Content = new MovieShop.Views.MovieShopView { ShowOnlySales = false };
            }
        }
    }
    }