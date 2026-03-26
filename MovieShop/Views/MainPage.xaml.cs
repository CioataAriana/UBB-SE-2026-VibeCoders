
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;


namespace MovieShop.Views
{
    public sealed partial class MainPage : UserControl
    {
        public ViewModels.FlashSaleViewModel FlashSaleVM => MovieShop.Services.SaleService.CurrentSale;
        private readonly MovieRepo _movieRepo = new();
        private readonly ActiveSalesRepo _salesRepo = new();
        private List<Movie> _sourceMovies = new();
        private Dictionary<int, int> _reviewCountByMovieId = new();

        public MainPage()
        {
            InitializeComponent();

            FlashSaleVM.PropertyChanged += FlashSaleVM_PropertyChanged!;
            UpdateBigBanner(FlashSaleVM.TimerText, FlashSaleVM.IsActive);

            SearchBox.TextChanged += (_, _) => ApplyFilterAndSort();
            SortAscPrice.IsChecked = true;
            RefreshUndiscountedMovies();
        }

        public void UpdateBigBanner(string time, bool isActive)
        {
            BigTimerText.Text = time;
            BigSaleBanner.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.XamlRoot.Content is NavigationPage navPage)
            {
                navPage.ViewModel.CurrentViewModel = "SalesPage";
            }

        }

        private void FlashSaleVM_PropertyChanged(object senser, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateBigBanner(FlashSaleVM.TimerText, FlashSaleVM.IsActive);

                // Only refresh when the flash sale starts/ends (not every second).
                if (e.PropertyName == nameof(ViewModels.FlashSaleViewModel.IsActive))
                    RefreshUndiscountedMovies();
            });
        }

        private void RefreshUndiscountedMovies()
        {
            var all = _movieRepo.GetAllMovies();
            var discountMap = _salesRepo.GetBestDiscountPercentByMovieId();
            ActiveSalesRepo.ApplyBestDiscountsToMovies(all, discountMap);

            var onSaleIds = _salesRepo.GetCurrentSales()
                                      .Select(s => s.Movie.ID)
                                      .Distinct()
                                      .ToHashSet();

            var undiscounted = all.Where(m => !onSaleIds.Contains(m.ID)).ToList();
            var reviewCountMap = GetReviewCounts(undiscounted.Select(m => m.ID).Distinct().ToList());

            _sourceMovies = undiscounted;
            _reviewCountByMovieId = reviewCountMap;
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            var q = (SearchBox.Text ?? "").Trim().ToLower();
            var list = string.IsNullOrEmpty(q)
                ? _sourceMovies
                : _sourceMovies.Where(m => m.Title.ToLower().Contains(q));

            if (SortAscPrice.IsChecked == true) list = list.OrderBy(m => m.Price);
            else if (SortDescPrice.IsChecked == true) list = list.OrderByDescending(m => m.Price);
            else if (SortHighRating.IsChecked == true) list = list.OrderByDescending(m => m.Rating);
            else if (SortLowRating.IsChecked == true) list = list.OrderBy(m => m.Rating);
            else list = list.OrderBy(m => m.Title);

            var orderedMovies = list.ToList();
            UndiscountedMoviesGrid.ItemsSource = orderedMovies
                .Select(m => new MovieCatalogItem(m, _reviewCountByMovieId.TryGetValue(m.ID, out var c) ? c : 0))
                .ToList();
        }

        private void SortOption_Changed(object sender, RoutedEventArgs e) => ApplyFilterAndSort();

        private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border) return;

            border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 29, 185, 84));
            border.Background = new SolidColorBrush(Color.FromArgb(255, 48, 48, 48));
            border.RenderTransform = new ScaleTransform { ScaleX = 1.03, ScaleY = 1.03 };
        }

        private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border) return;

            border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64));
            border.Background = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42));
            border.RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        }

        private void UndiscountedMoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not MovieCatalogItem item)
                return;

            NavigateToMovieDetail(item.Movie);
        }

        private void NavigateToMovieDetail(Movie movie)
        {
            var navPage = FindAncestorNavigationPage();
            navPage?.NavigateToMovieDetail(movie, showOnlySales: false);
        }

        private NavigationPage? FindAncestorNavigationPage()
        {
            UIElement? current = this;
            while (current != null)
            {
                if (current is NavigationPage navPage)
                    return navPage;

                current = VisualTreeHelper.GetParent(current) as UIElement;
            }

            return null;
        }

        private static Dictionary<int, int> GetReviewCounts(List<int> movieIds)
        {
            var result = new Dictionary<int, int>();
            if (movieIds.Count == 0) return result;

            var db = DatabaseSingleton.Instance;
            db.OpenConnection();

            try
            {
                var paramNames = movieIds.Select((_, i) => $"@id{i}").ToArray();
                var inClause = string.Join(",", paramNames);
                var query = $@"SELECT MovieID
                                FROM Reviews
                                WHERE MovieID IN ({inClause})";

                using var cmd = new SqlCommand(query, db.Connection);
                for (var i = 0; i < movieIds.Count; i++)
                    cmd.Parameters.AddWithValue(paramNames[i], movieIds[i]);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var movieId = reader.GetInt32(0);
                    result.TryGetValue(movieId, out var cnt);
                    result[movieId] = cnt + 1;
                }
            }
            finally
            {
                db.CloseConnection();
            }

            return result;
        }
    }
}