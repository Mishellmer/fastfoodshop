using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using fastfoodshop.Models;
using fastfoodshop.Services;

namespace fastfoodshop.ViewModels
{
    // Управление меню и стоп-листом (доступно менеджеру и администратору).
    public class MenuViewModel : ViewModelBase
    {
        private readonly MenuService _menuService = new();

        private MenuItem? _selectedItem;
        private Category? _selectedCategory;
        private string _name = string.Empty;
        private string _priceText = string.Empty;
        private bool _isAvailable = true;
        private string _statusMessage = "Выберите позицию меню или добавьте новую.";

        public ObservableCollection<MenuItem> Items { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public MenuItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!SetProperty(ref _selectedItem, value)) return;
                LoadFields();
            }
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string PriceText
        {
            get => _priceText;
            set => SetProperty(ref _priceText, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ToggleAvailabilityCommand { get; }
        public ICommand RefreshCommand { get; }

        public MenuViewModel()
        {
            AddCommand = new RelayCommand(Add);
            SaveCommand = new RelayCommand(Save);
            DeleteCommand = new RelayCommand(Delete);
            ToggleAvailabilityCommand = new RelayCommand(ToggleAvailability);
            RefreshCommand = new RelayCommand(Load);

            Load();
        }

        public void Load()
        {
            try
            {
                var categories = _menuService.GetCategories();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                var items = _menuService.GetAll();
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }

                _selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                LoadFields();

                StatusMessage = $"Загружено позиций: {Items.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки меню: " + GetErrorMessage(ex);
            }
        }

        private void LoadFields()
        {
            if (SelectedItem == null)
            {
                Name = string.Empty;
                PriceText = string.Empty;
                SelectedCategory = Categories.FirstOrDefault();
                IsAvailable = true;
                return;
            }

            Name = SelectedItem.ItemName;
            PriceText = SelectedItem.Price.ToString("N2", CultureInfo.CurrentCulture);
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedItem.CategoryId);
            IsAvailable = SelectedItem.IsAvailable;
        }

        private void Add()
        {
            try
            {
                if (SelectedCategory == null)
                    throw new InvalidOperationException("Выберите категорию блюда.");

                var created = _menuService.Create(Name, ParsePrice(), SelectedCategory.CategoryId, IsAvailable);

                Load();
                SelectedItem = Items.FirstOrDefault(i => i.MenuItemId == created.MenuItemId);
                StatusMessage = $"Добавлена позиция «{created.ItemName}».";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка добавления: " + GetErrorMessage(ex);
            }
        }

        private void Save()
        {
            if (SelectedItem == null)
            {
                StatusMessage = "Сначала выберите позицию меню.";
                return;
            }

            try
            {
                if (SelectedCategory == null)
                    throw new InvalidOperationException("Выберите категорию блюда.");

                var id = SelectedItem.MenuItemId;
                _menuService.Update(id, Name, ParsePrice(), SelectedCategory.CategoryId, IsAvailable);

                Load();
                SelectedItem = Items.FirstOrDefault(i => i.MenuItemId == id);
                StatusMessage = "Изменения сохранены.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка сохранения: " + GetErrorMessage(ex);
            }
        }

        private void Delete()
        {
            if (SelectedItem == null)
            {
                StatusMessage = "Сначала выберите позицию меню.";
                return;
            }

            var deletedName = SelectedItem.ItemName;

            try
            {
                _menuService.Delete(SelectedItem.MenuItemId);
                Load();
                StatusMessage = $"Позиция «{deletedName}» удалена.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка удаления: " + GetErrorMessage(ex);
            }
        }

        // Быстрое управление стоп-листом: снять с продажи или вернуть в меню.
        private void ToggleAvailability()
        {
            if (SelectedItem == null)
            {
                StatusMessage = "Сначала выберите позицию меню.";
                return;
            }

            try
            {
                var id = SelectedItem.MenuItemId;
                var newState = !SelectedItem.IsAvailable;

                _menuService.SetAvailability(id, newState);

                Load();
                SelectedItem = Items.FirstOrDefault(i => i.MenuItemId == id);

                StatusMessage = newState
                    ? "Позиция возвращена в продажу."
                    : "Позиция добавлена в стоп-лист.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка изменения стоп-листа: " + GetErrorMessage(ex);
            }
        }

        private decimal ParsePrice()
        {
            var text = (PriceText ?? string.Empty).Trim();

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
                return price;

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                return price;

            throw new InvalidOperationException("Укажите корректную цену (например, 149,00).");
        }

        private static string GetErrorMessage(Exception ex)
        {
            var current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }
    }
}
