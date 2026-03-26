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
    }

    private void UpdatePriceDisplay()
    {
        if (_movie == null)
            return;

        PriceBlock.Inlines.Clear();

        var label = new Run
        {
            Text = "Price: ",
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
        };
        PriceBlock.Inlines.Add(label);

        var only = new Run
        {
            Text = _movie.Price.ToString("C"),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 29, 185, 84)),
            FontWeight = FontWeights.Bold
        };
        PriceBlock.Inlines.Add(only);
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
            Content = $"Buy \"{_movie.Title}\" for {_movie.Price:C}? This will be charged to your balance.",
            PrimaryButtonText = "Buy",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
            return;

        try
        {
            await Task.Run(() => _movieRepo.PurchaseMovie(SessionManager.CurrentUserID, _movie.ID));

            _mainVm.RefreshBalanceFromDatabase();
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

    private async void ReviewsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null)
            return;

        var reviewCount = GetReviewCount(_movie.ID);
        var peopleText = reviewCount == 1 ? "1 person" : $"{reviewCount} people";

        var dialog = new ContentDialog
        {
            Title = "Reviews",
            Content = $"\"{_movie.Title}\" was reviewed by {peopleText}.",
            PrimaryButtonText = "Close",
            XamlRoot = XamlRoot
        };
        _ = await dialog.ShowAsync();
    }

    private async void EventsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Related upcoming events",
            Content = $"Upcoming events linked to \"{_movie.Title}\" will be shown here (REQ-01 — Events integration).",
            PrimaryButtonText = "Close",
            XamlRoot = XamlRoot
        };
        _ = await dialog.ShowAsync();
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
            return;
        }

        // If this page was opened directly (no back stack), return to the MainPage (Shop view).
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
}
