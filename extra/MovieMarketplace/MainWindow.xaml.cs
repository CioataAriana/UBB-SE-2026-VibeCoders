using Microsoft.UI.Xaml;

namespace MovieMarketplace
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            App.m_window = this;

            RootFrame.Navigate(typeof(MovieMarketplace.Views.StartPage));
        }
    }
}