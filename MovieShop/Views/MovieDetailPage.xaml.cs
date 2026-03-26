using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Data.SqlClient;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace MovieShop.Views;

public sealed partial class MovieDetailPage : Page
{
    private Movie? _movie;
    private MainViewModel? _mainVm;
    private readonly MovieRepo _movieRepo = new();

    public MovieDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieDetailNavArgs args)
            return;

        _movie = args.Movie;
        _mainVm = args.MainViewModel;

        if (_movie == null)
            return;

        var discountMap = new ActiveSalesRepo().GetBestDiscountPercentByMovieId();
        ActiveSalesRepo.ApplyBestDiscountsToMovies(new List<Movie> { _movie }, discountMap);

        TitleBlock.Text = _movie.Title;
        DescriptionBlock.Text = string.IsNullOrEmpty(_movie.Description) ? "—" : _movie.Description;
        RatingBlock.Text = $"Rating: {_movie.Rating:0.0} / 10";
        UpdatePriceDisplay();

        TrySetPoster(_movie.ImageUrl);

        RefreshBuyButtonState();

        ToolTipService.SetToolTip(ReviewsButton, BuildStarDistributionTooltip(_movie.ID));
    }

    private void UpdatePriceDisplay()
    {
        if (_movie == null)
            return;

        PriceBlock.Text = $"${_movie.DiscountedPriceText}";

        if (_movie.HasActiveSale)
        {
            OriginalPriceBlock.Visibility = Visibility.Visible;
            OriginalPriceBlock.Text = $"${_movie.OriginalPriceText}";
        }
        else
        {
            OriginalPriceBlock.Visibility = Visibility.Collapsed;
        }
    }

    private void TrySetPoster(string? url)
    {
        PosterImage.Source = null;
        if (string.IsNullOrWhiteSpace(url))
            return;
        try
        {
            PosterImage.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
        }
        catch
        {
            /* ignore invalid image URL */
        }
    }

    private void RefreshBuyButtonState()
    {
        if (_movie == null)
            return;

        _mainVm?.RefreshBalanceFromDatabase();
        var userId = SessionManager.CurrentUserID;
        var loggedIn = SessionManager.IsLoggedIn;
        var owned = _movieRepo.UserOwnsMovie(userId, _movie.ID);
        var balance = _mainVm?.Balance ?? SessionManager.CurrentUserBalance;

        var insufficient = loggedIn && !owned && balance < _movie.GetEffectivePrice();

        if (owned)
        {
            BuyMovieButton.Content = "Owned";
            BuyMovieButton.IsEnabled = false;
            ToolTipService.SetToolTip(BuyMovieButton, null);
            BuyMovieButton.Opacity = 1;
            return;
        }

        BuyMovieButton.Content = "Buy movie";

        if (!loggedIn)
        {
            BuyMovieButton.IsEnabled = false;
            ToolTipService.SetToolTip(BuyMovieButton, "You must be logged in to make a purchase.");
            BuyMovieButton.Opacity = 0.55;
            return;
        }

        if (insufficient)
        {
            BuyMovieButton.IsEnabled = false;
            ToolTipService.SetToolTip(BuyMovieButton, "Your balance is too low to purchase this movie.");
            BuyMovieButton.Opacity = 0.55;
            return;
        }

        BuyMovieButton.IsEnabled = true;
        ToolTipService.SetToolTip(BuyMovieButton, null);
        BuyMovieButton.Opacity = 1;
    }

    private async void BuyMovieButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null || _mainVm == null)
            return;

        if (!SessionManager.IsLoggedIn)
            return;

        RefreshBuyButtonState();
        if (!BuyMovieButton.IsEnabled)
            return;

        var confirm = new ContentDialog
        {
            Title = "Confirm purchase",
            Content = $"Buy \"{_movie.Title}\" for {_movie.GetEffectivePrice():C}? This will be charged to your balance.",
            PrimaryButtonText = "Buy",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
            return;

        try
        {
            await Task.Run(() => _movieRepo.PurchaseMovie(SessionManager.CurrentUserID, _movie.ID, _movie.GetEffectivePrice()));

            // Refresh balance in nav bar AND reload wallet transaction list
            _mainVm.RefreshWallet();
            SessionManager.CurrentUserBalance = _mainVm.Balance;

            RefreshBuyButtonState();

            var dialog = new ContentDialog
            {
                Title = "Purchase successful",
                Content = $"You now own \"{_movie.Title}\". It has been added to your inventory.",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            _ = await dialog.ShowAsync();

            if (this.XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.CurrentViewModel = "Inventory";
            }
        }
        catch (InvalidOperationException ex)
        {
            var err = new ContentDialog
            {
                Title = "Cannot complete purchase",
                Content = ex.Message,
                PrimaryButtonText = "OK",
                XamlRoot = XamlRoot
            };
            _ = await err.ShowAsync();
        }
    }

    private void ReviewsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null || _mainVm == null)
            return;

        Frame?.Navigate(typeof(MovieReviewsPage), new MovieReviewsNavArgs
        {
            Movie = _movie,
            MainViewModel = _mainVm
        });
    }

    private void EventsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null || _mainVm == null)
            return;

        Frame?.Navigate(typeof(MovieEventsPage), new MovieEventsNavArgs
        {
            Movie = _movie,
            MainViewModel = _mainVm
        });
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
            return;
        }

        if (this.XamlRoot?.Content is NavigationPage navPage)
        {
            navPage.ViewModel.CurrentViewModel = "Shop";
        }
    }

    private static int GetReviewCount(int movieId)
    {
        var db = DatabaseSingleton.Instance;
        db.OpenConnection();

        try
        {
            const string query = @"SELECT StarRating FROM Reviews WHERE MovieID = @mid";
            using var cmd = new SqlCommand(query, db.Connection);
            cmd.Parameters.AddWithValue("@mid", movieId);

            using var reader = cmd.ExecuteReader();
            var count = 0;
            while (reader.Read())
                count++;

            return count;
        }
        finally
        {
            db.CloseConnection();
        }
    }

    private static string BuildStarDistributionTooltip(int movieId)
    {
        var counts = new int[11];

        var db = DatabaseSingleton.Instance;
        db.OpenConnection();
        try
        {
            const string query = @"SELECT StarRating FROM Reviews WHERE MovieID = @mid";
            using var cmd = new SqlCommand(query, db.Connection);
            cmd.Parameters.AddWithValue("@mid", movieId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var rating = reader.GetInt32(0);
                var bucket = (int)decimal.Floor(rating);
                if (bucket < 1) bucket = 1;
                if (bucket > 10) bucket = 10;
                counts[bucket]++;
            }
        }
        finally
        {
            db.CloseConnection();
        }

        var total = 0;
        for (var i = 1; i <= 10; i++) total += counts[i];
        if (total == 0) return "No reviews yet.";

        var lines = new List<string> { "Rating distribution:" };
        for (var i = 10; i >= 1; i--)
            lines.Add($"{i}: {counts[i]}");

        return string.Join("\n", lines);
    }
}