using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace MovieShop.Views
{
    public sealed partial class MainPage : UserControl
    {
        public ViewModels.FlashSaleViewModel FlashSaleVM => MovieShop.Services.SaleService.CurrentSale;
        public MainPage()
        {
            InitializeComponent();

        }

        public void UpdateBigBanner(string time, bool isActive)
        {
            BigTimerText.Text = time;
            BigSaleBanner.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.XamlRoot.Content is NavigationPage navPage)
            {
                // 2. Tell the "Boss" ViewModel to switch to the SalesPage state
                navPage.ViewModel.CurrentViewModel = "SalesPage";

                // This triggers the 'else if' you just wrote in NavigationPage!
            }

        }
     }
}
