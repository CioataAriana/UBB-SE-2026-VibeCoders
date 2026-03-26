using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using MovieShop.Models;       
using MovieShop.Repositories; 

namespace MovieShop.ViewModels 
{
    public class MarketplaceViewModel : INotifyPropertyChanged
    {
        private readonly EquipmentRepo _repository = new EquipmentRepo();

        private List<Equipment> _allOriginalItems = new List<Equipment>();

        public ObservableCollection<Equipment> AvailableItems { get; set; } = new ObservableCollection<Equipment>();

        public decimal UserBalance
        {
            get
            {
                // Aici poți returna balanța reală dacă ai un WalletManager în MovieShop
                return 5000.00m;
            }
        }

        public MarketplaceViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            // Apelăm metoda FetchAvailableEquipment din noul repo
            var data = _repository.FetchAvailableEquipment() ?? new List<Equipment>();
            _allOriginalItems = data;

            UpdateDisplayList(_allOriginalItems);
        }

        public void FilterByCategory(string? category)
        {
            // Logica de filtrare rămâne identică
            var filtered = string.IsNullOrEmpty(category) || category == "All"
                ? _allOriginalItems
                : _allOriginalItems.Where(x => x.Category == category).ToList();

            UpdateDisplayList(filtered);
        }

        private void UpdateDisplayList(List<Equipment> items)
        {
            AvailableItems.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    AvailableItems.Add(item);
                }
            }
            // Notificăm interfața că lista s-a schimbat
            OnPropertyChanged(nameof(AvailableItems));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Verificăm sesiunea folosind SessionManager-ul din MovieShop
        public string StatusMessage => SessionManager.CurrentUserID == 0
            ? "Please log in to purchase equipment."
            : string.Empty;
    }
}