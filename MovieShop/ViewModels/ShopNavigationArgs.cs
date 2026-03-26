using MovieShop.Models;

namespace MovieShop.ViewModels
{
    public sealed class MovieCatalogNavArgs
    {
        public MainViewModel MainViewModel { get; init; } = null!;
        public bool ShowOnlySales { get; init; }
    }

    public sealed class MovieDetailNavArgs
    {
        public Movie Movie { get; init; } = null!;
        public MainViewModel MainViewModel { get; init; } = null!;
    }
}
