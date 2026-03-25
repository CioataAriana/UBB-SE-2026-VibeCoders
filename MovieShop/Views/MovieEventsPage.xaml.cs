using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed class MovieEventCardRow
    {
        public string Title { get; set; } = "";
        public string WhenWhere { get; set; } = "";
        public string Description { get; set; } = "";
        public string PriceLine { get; set; } = "";
        public Microsoft.UI.Xaml.Media.ImageSource? PosterSource { get; set; }
    }

    public sealed partial class MovieEventsPage : Page
    {
        private readonly Movie _movie;
        private readonly bool _catalogShowOnlySales;
        private readonly EventService _eventService = new EventService();

        public MovieEventsPage(Movie movie, bool catalogShowOnlySales = false)
        {
            InitializeComponent();
            _movie = movie;
            _catalogShowOnlySales = catalogShowOnlySales;
            Loaded += MovieEventsPage_Loaded;
        }

        private void MovieEventsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            HeaderText.Text = $"Upcoming events — {_movie.Title}";
            var events = _eventService.GetUpcomingForMovie(_movie.ID);
            var rows = new List<MovieEventCardRow>();
            foreach (var ev in events)
            {
                BitmapImage? bmp = null;
                if (!string.IsNullOrWhiteSpace(ev.PosterUrl))
                {
                    try
                    {
                        bmp = new BitmapImage(new Uri(ev.PosterUrl));
                    }
                    catch
                    {
                        /* ignore bad URL */
                    }
                }

                rows.Add(new MovieEventCardRow
                {
                    Title = ev.Title,
                    WhenWhere = $"{ev.Date:g} · {ev.Location}",
                    Description = ev.Description,
                    PriceLine = $"Tickets from ${ev.TicketPrice:F2}",
                    PosterSource = bmp
                });
            }

            EventsItemsControl.ItemsSource = rows;
        }

        private void BackToPurchase_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MovieShopNavigation.SetMainContent(this, new MovieDetailPage(_movie, _catalogShowOnlySales));
        }
    }
}
