using BoardRent.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BoardRent.Views
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterPage()
        {
            this.InitializeComponent();

            ViewModel = Ioc.Default.GetService<RegisterViewModel>();

            this.DataContext = ViewModel;
        }
    }
}