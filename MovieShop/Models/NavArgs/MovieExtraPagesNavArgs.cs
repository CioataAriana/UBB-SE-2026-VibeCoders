using MovieShop.ViewModels;

namespace MovieShop.Models
{
    public sealed class MovieReviewsNavArgs
    {
        public MainViewModel MainViewModel { get; init; } = null!;
        public Movie Movie { get; init; } = null!;
    }

    public sealed class MovieEventsNavArgs
    {
        public MainViewModel? MainViewModel { get; init; }
        public Movie? Movie { get; init; }
    }
}

