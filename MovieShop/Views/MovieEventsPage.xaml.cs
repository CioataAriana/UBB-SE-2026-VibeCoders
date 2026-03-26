using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;

namespace MovieShop.Views;

    public sealed partial class MovieEventsPage : Page
{
    private Movie? _movie;
    private List<MovieEvent>? _allEvents;

        private static object? FindDescendantByName(DependencyObject parent, string name)
        {
            if (parent == null) return null;
            var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == name)
                    return fe;
                var found = FindDescendantByName(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

    public MovieEventsPage()
    {
        InitializeComponent();
        this.Loaded += MovieEventsPage_Loaded;
    }

    private void MovieEventsPage_Loaded(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // If the page was created directly (e.g. from the main nav) OnNavigatedTo won't be called.
        // In that case load all events so the Tickets nav shows content.
        if (EventsList.ItemsSource == null)
        {
            TitleBlock.Text = "Events";
            _allEvents = new EventRepo().GetAllEvents();
            EventsList.ItemsSource = _allEvents;
            UpdateBuyButtons();
        }
    }

    private void SearchBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void FilterCombo_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_allEvents == null)
            return;

        var filtered = _allEvents.AsEnumerable();

        var search = SearchBox?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(ev => (ev.Title ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || (ev.Description ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || (ev.Location ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

        var sel = (FilterCombo?.SelectedItem as Microsoft.UI.Xaml.Controls.ComboBoxItem)?.Content as string ?? "All";
        if (sel == "Upcoming")
            filtered = filtered.Where(ev => ev.Date >= DateTime.Now);
        else if (sel == "Past")
            filtered = filtered.Where(ev => ev.Date < DateTime.Now);

        EventsList.ItemsSource = filtered.ToList();
        UpdateBuyButtons();
    }

    private void UpdateBuyButtons()
    {
        // Ensure buttons are disabled when user not logged in or insufficient funds
        var userId = SessionManager.CurrentUserID;
        var balance = userId > 0 ? new UserRepo().GetBalance(userId) : 0m;

        foreach (var item in EventsList.Items)
        {
            var lvi = EventsList.ContainerFromItem(item) as ListViewItem;
            if (lvi == null) continue;
            var btnObj = FindDescendantByName(lvi, "BuyTicketButton") as Button;
            if (btnObj == null) continue;

            var ev = item as MovieEvent;
            if (ev == null)
            {
                btnObj.IsEnabled = false;
                btnObj.Opacity = 0.5;
                continue;
            }

            if (userId <= 0 || balance < ev.TicketPrice)
            {
                btnObj.IsEnabled = false;
                btnObj.Opacity = 0.55;
            }
            else
            {
                btnObj.IsEnabled = true;
                btnObj.Opacity = 1;
            }
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieEventsNavArgs args)
            return;

        _movie = args.Movie;
        if (_movie == null)
        {
            TitleBlock.Text = "Events";
            var eventsAll = new EventRepo().GetAllEvents();
            EventsList.ItemsSource = eventsAll;
            return;
        }

        TitleBlock.Text = $"Events - {_movie.Title}";
        var events = new EventRepo().GetEventsForMovie(_movie.ID);
        EventsList.ItemsSource = events;
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
            Frame.GoBack();
    }

    private void BuyTicket_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // sender's DataContext should be MovieEvent from the ItemTemplate
        if (sender is Microsoft.UI.Xaml.FrameworkElement fe && fe.DataContext is MovieEvent me)
        {
            // If user not logged in, don't navigate to buy page (no login flow requested)
            if (!Models.SessionManager.IsLoggedIn)
            {
                // Optionally show a message inline in the events list - for now do nothing
                return;
            }

            Frame?.Navigate(typeof(BuyTicketPage), me.ID);
        }
    }
}

