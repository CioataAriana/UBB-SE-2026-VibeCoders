using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Repositories;
using MovieShop.Models;
using System.Collections.Generic;

namespace MovieShop.Views
{
    public sealed partial class InventoryView : Page
    {
        private readonly InventoryRepo _repo = new InventoryRepo();

        public InventoryView()
        {
            InitializeComponent();
            Loaded += InventoryView_Loaded;
            MoviesGrid.ItemClick += MoviesGrid_ItemClick;
        }

        private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie m)
            {
                Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs { Movie = m, MainViewModel = null! });
            }
        }

        private void InventoryView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var userId = SessionManager.CurrentUserID;
            if (userId <= 0) return;

            var movies = _repo.GetOwnedMovies(userId);
            MoviesGrid.ItemsSource = movies;

            var tickets = _repo.GetOwnedTickets(userId);
            TicketsGrid.ItemsSource = tickets;

            var equipment = _repo.GetOwnedEquipment(userId);
            EquipmentGrid.ItemsSource = equipment;
        }
    }
}
