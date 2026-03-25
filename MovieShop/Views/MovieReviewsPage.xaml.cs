using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed partial class MovieReviewsPage : Page
    {
        private readonly Movie _movie;
        private readonly bool _catalogShowOnlySales;
        private readonly ReviewService _reviewService = new ReviewService();

        public MovieReviewsPage(Movie movie, bool catalogShowOnlySales = false)
        {
            InitializeComponent();
            _movie = movie;
            _catalogShowOnlySales = catalogShowOnlySales;
            Loaded += MovieReviewsPage_Loaded;
        }

        private void MovieReviewsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            HeaderText.Text = $"Reviews — {_movie.Title}";
            _reviewService.EnsureTwoSampleReviewsIfEmpty(_movie.ID);
            var reviews = _reviewService.GetReviewsForMovie(_movie.ID);
            var rows = reviews.Select(r => new ReviewListRow
            {
                UserDisplayName = r.UserDisplayName,
                StarRating = r.StarRating,
                CommentLine = string.IsNullOrWhiteSpace(r.Comment) ? "(No written comment)" : r.Comment,
                CreatedAtDisplay = r.CreatedAt.ToString("g")
            }).ToList();
            ReviewsItemsControl.ItemsSource = rows;
        }

        private void BackToPurchase_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MovieShopNavigation.SetMainContent(this, new MovieDetailPage(_movie, _catalogShowOnlySales));
        }
    }
}
