using Microsoft.UI.Xaml;

namespace MovieShop.Views
{
    internal static class MovieShopNavigation
    {
        public static void SetMainContent(FrameworkElement context, object content)
        {
            if (context.XamlRoot?.Content is NavigationPage nav)
                nav.SetMainContent(content);
        }
    }
}
