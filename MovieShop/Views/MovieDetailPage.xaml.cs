using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed partial class MovieDetailPage : Page
    {
        private readonly Movie _movie;
        private readonly bool _catalogShowOnlySales;
        private readonly ReviewService _reviewService = new ReviewService();
        private readonly EventService _eventService = new EventService();
        private ReviewSummary _reviewSummary = new ReviewSummary();

        public MovieDetailPage(Movie movie, bool catalogShowOnlySales = false)
        {
            InitializeComponent();
            _movie = movie ?? throw new ArgumentNullException(nameof(movie));
            _catalogShowOnlySales = catalogShowOnlySales;
            Loaded += MovieDetailPage_Loaded;
        }

        private void MovieDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            TitleLabel.Text = _movie.Title;
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(_movie.Description)
                ? "No description available."
                : _movie.Description;
            PriceLabel.Text = $"${_movie.Price:F2}";

            if (!string.IsNullOrEmpty(_movie.ImageUrl))
            {
                try
                {
                    PosterImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(_movie.ImageUrl));
                }
                catch
                {
                    PosterImage.Source = null;
                }
            }

            _reviewService.EnsureTwoSampleReviewsIfEmpty(_movie.ID);
            _reviewSummary = _reviewService.GetSummaryForMovie(_movie.ID);
            if (_reviewSummary.Count > 0)
            {
                string reviewWord = _reviewSummary.Count == 1 ? "Review" : "Reviews";
                ReviewsButton.Content = $"{_reviewSummary.AverageRounded:0.0} ({_reviewSummary.Count} {reviewWord})";
                RatingLabel.Text = $"Rating (from reviews): {_reviewSummary.AverageRounded:0.0} / 5";
            }
            else
            {
                ReviewsButton.Content = "See reviews";
                RatingLabel.Text = $"Rating: {_movie.Rating:0.0} / 5";
            }

            ToolTipService.SetToolTip(ReviewsButton, ReviewService.BuildStarDistributionTooltip(_reviewSummary));

            int upcoming = _eventService.CountUpcomingForMovie(_movie.ID);
            EventsButton.Visibility = upcoming > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BuyMovieButton_Click(object sender, RoutedEventArgs e)
        {
            _ = new ContentDialog
            {
                Title = "Buy Movie",
                Content = $"Purchase \"{_movie.Title}\" is not completed in this flow.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            MovieShopNavigation.SetMainContent(this, new MovieReviewsPage(_movie, _catalogShowOnlySales));
        }

        private void EventsButton_Click(object sender, RoutedEventArgs e)
        {
            MovieShopNavigation.SetMainContent(this, new MovieEventsPage(_movie, _catalogShowOnlySales));
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            MovieShopNavigation.SetMainContent(this, new MovieShopView { ShowOnlySales = _catalogShowOnlySales });
        }
    }
}
