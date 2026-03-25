using Microsoft.UI.Xaml.Media;
using MovieShop.Models;

namespace MovieShop.Views
{
    /// <summary>Grid item for the shop: movie data plus an optional poster <see cref="ImageSource"/>.</summary>
    public sealed class MovieCatalogRow
    {
        public Movie Movie { get; }
        public string Title => Movie.Title;
        public decimal Price => Movie.Price;
        public double Rating => Movie.Rating;
        public ImageSource? Poster { get; }

        public MovieCatalogRow(Movie movie, ImageSource? poster)
        {
            Movie = movie;
            Poster = poster;
        }
    }
}
