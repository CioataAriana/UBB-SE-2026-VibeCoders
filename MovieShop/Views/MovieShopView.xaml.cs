using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;
using MovieShop.Models;

namespace MovieShop.Views
{
    public sealed partial class MovieShopView : UserControl
    {
        public bool ShowOnlySales { get; set; }

        public MainViewModel? HostViewModel { get; set; }

        public Movie? InitialMovie { get; set; }

        public MovieShopView()
        {
            InitializeComponent();
            Loaded += MovieShopView_Loaded;
        }

        private void MovieShopView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MovieShopView_Loaded;

            if (HostViewModel == null)
                return;

            if (InitialMovie != null)
            {
                ShopFrame.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs
                {
                    MainViewModel = HostViewModel,
                    Movie = InitialMovie
                });
                return;
            }

            ShopFrame.Navigate(typeof(MovieCatalogPage), new MovieCatalogNavArgs
            {
                MainViewModel = HostViewModel,
                ShowOnlySales = ShowOnlySales
            });
        }
    }
}
