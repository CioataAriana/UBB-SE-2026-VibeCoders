using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MovieMarketplace.ViewModels
{
    public class SellViewModel : INotifyPropertyChanged
    {
        private string _newItemTitle = string.Empty;
        private string _newItemPrice = string.Empty;
        private string _newItemDesc = string.Empty;

        public string NewItemTitle
        {
            get => _newItemTitle;
            set { _newItemTitle = value; OnPropertyChanged(); }
        }

        public string NewItemPrice
        {
            get => _newItemPrice;
            set { _newItemPrice = value; OnPropertyChanged(); }
        }

        public string NewItemDesc
        {
            get => _newItemDesc;
            set { _newItemDesc = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}