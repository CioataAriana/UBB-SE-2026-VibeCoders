using MovieShop.ViewModels;

namespace MovieShop.Models
{
    public sealed class MovieCatalogNavArgs
    {
        public MainViewModel MainViewModel { get; init; } = null!;

        public bool ShowOnlySales { get; init; }
    }

    public sealed class MovieDetailNavArgs
    {
        public MainViewModel MainViewModel { get; init; } = null!;

        public Movie Movie { get; init; } = null!;
    }
}
