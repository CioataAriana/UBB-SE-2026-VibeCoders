using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Views
{
    public sealed partial class MovieShopView : UserControl
    {
        public bool ShowOnlySales { get; set; }

        private readonly MovieRepo _movieRepo = new MovieRepo();
        private List<Movie> _allMovies = new List<Movie>();
        private string _sortKey = "";

        public MovieShopView()
        {
            InitializeComponent();
            Loaded += MovieShopView_Loaded;
        }

        private void MovieShopView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                _allMovies = ShowOnlySales
                    ? _movieRepo.GetMoviesWithActiveSale()
                    : _movieRepo.GetAllMovies();
            }
            catch
            {
                _allMovies = new List<Movie>();
            }

            ApplyFilterAndSort();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void SortRadio_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                _sortKey = tag;
                ApplyFilterAndSort();
            }
        }

        private void ApplyFilterAndSort()
        {
            var q = (SearchBox?.Text ?? "").Trim();
            IEnumerable<Movie> seq = _allMovies;
            if (!string.IsNullOrEmpty(q))
                seq = seq.Where(m => m.Title != null && m.Title.Contains(q, System.StringComparison.OrdinalIgnoreCase));

            seq = _sortKey switch
            {
                "price_asc" => seq.OrderBy(m => m.Price),
                "price_desc" => seq.OrderByDescending(m => m.Price),
                "rating_desc" => seq.OrderByDescending(m => m.Rating),
                "rating_asc" => seq.OrderBy(m => m.Rating),
                _ => seq.OrderBy(m => m.Title)
            };

            MoviesGridView.ItemsSource = seq.Select(m => new MovieCatalogRow(m, TryLoadPoster(m.ImageUrl))).ToList();
        }

        private static Microsoft.UI.Xaml.Media.ImageSource? TryLoadPoster(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                return new BitmapImage(new Uri(url.Trim()));
            }
            catch
            {
                return null;
            }
        }

        private void MoviesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MovieCatalogRow row)
                MovieShopNavigation.SetMainContent(this, new MovieDetailPage(row.Movie, ShowOnlySales));
        }
    }
}
