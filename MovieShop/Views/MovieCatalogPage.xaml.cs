using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace MovieShop.Views;

public sealed partial class MovieCatalogPage : Page
{
    private readonly MovieRepo _movieRepo = new();
    private readonly ActiveSalesRepo _salesRepo = new();
    private List<Movie> _sourceMovies = new();
    private MainViewModel? _mainVm;
    private bool _showOnlySales;

    public MovieCatalogPage()
    {
        InitializeComponent();
        SearchBox.TextChanged += (_, _) => ApplyFilterAndSort();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MovieCatalogNavArgs args)
        {
            _mainVm = args.MainViewModel;
            _showOnlySales = args.ShowOnlySales;
        }

        var all = _movieRepo.GetAllMovies();
        var discountMap = _salesRepo.GetBestDiscountPercentByMovieId();
        ActiveSalesRepo.ApplyBestDiscountsToMovies(all, discountMap);

        if (_showOnlySales)
        {
            var onSaleIds = _salesRepo.GetCurrentSales()
                .Select(s => s.Movie.ID)
                .Distinct()
                .ToHashSet();
            _sourceMovies = all.Where(m => onSaleIds.Contains(m.ID)).ToList();
        }
        else
        {
            _sourceMovies = all;
        }

        SortAscPrice.IsChecked = true;
        ApplyFilterAndSort();
    }

    private void SortOption_Changed(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        ApplyFilterAndSort();

    private void ApplyFilterAndSort()
    {
        var q = (SearchBox.Text ?? "").Trim().ToLowerInvariant();
        IEnumerable<Movie> list = string.IsNullOrEmpty(q)
            ? _sourceMovies
            : _sourceMovies.Where(m => m.Title.ToLowerInvariant().Contains(q));

        if (SortAscPrice.IsChecked == true)
            list = list.OrderBy(m => m.GetEffectivePrice());
        else if (SortDescPrice.IsChecked == true)
            list = list.OrderByDescending(m => m.GetEffectivePrice());
        else if (SortHighRating.IsChecked == true)
            list = list.OrderByDescending(m => m.Rating);
        else if (SortLowRating.IsChecked == true)
            list = list.OrderBy(m => m.Rating);
        else
            list = list.OrderBy(m => m.Title);

        MoviesGrid.ItemsSource = list.ToList();
    }

    private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Movie movie || _mainVm == null)
            return;

        Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs
        {
            Movie = movie,
            MainViewModel = _mainVm
        });
    }
}
