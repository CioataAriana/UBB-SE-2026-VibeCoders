using Microsoft.UI.Xaml;
using MovieShop.ViewModels;
using MovieShop.Views;

namespace MovieShop
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var mainVM = new MainWindowViewModel();

            MovieShop.Services.SaleService.CurrentSale = mainVM.FlashSaleVM;
            GlobalSaleBanner.Visibility = Visibility.Collapsed;

            MainContentArea.Content = new MainPage();

        }

        public void ShowMovieShopPage()
        {
            GlobalSaleBanner.Visibility=Visibility.Visible;
            MainContentArea.Content = new MovieShopView();
        }
       
    }
}