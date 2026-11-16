using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;
using fastfoodshop.Models;
using fastfoodshop.Services;

namespace fastfoodshop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DiscountService _discountService = new();
        private readonly OrderService _orderService = new();
        private readonly MenuService _menuService = new();
        private readonly List<MenuItem> _allMenuItems = new();
        private readonly RelayCommand _closeOrderCommand;

        private string _dbStatus = "Подключение...";
        private decimal _totalSum;
        private decimal _subTotal;
        private decimal _discountAmount;
        private string _discountText = "Скидка не применяется";
        private string _statusMessage = "Готово к работе.";
        private string _searchText = string.Empty;
        private decimal? _quantity = 1;
        private Client? _selectedClient;
        private int _lastOrderId;

        public Employee CurrentUser { get; }
        public ObservableCollection<MenuItem> MenuItems { get; } = new();
        public ObservableCollection<OrderItem> Cart { get; } = new();
        public ObservableCollection<Client> Clients { get; } = new();

        // Свойства для скрытия кнопок по ролям
        public bool CanManageMenu => CurrentUser?.Role?.RoleName is "Менеджер" or "Администратор";
        public bool CanManageUsers => CurrentUser?.Role?.RoleName == "Администратор";

        public string DbStatus { get => _dbStatus; set { _dbStatus = value; OnPropertyChanged(); } }
        public decimal TotalSum { get => _totalSum; set { _totalSum = value; OnPropertyChanged(); } }
        public decimal SubTotal { get => _subTotal; set { _subTotal = value; OnPropertyChanged(); } }
        public decimal DiscountAmount { get => _discountAmount; set { _discountAmount = value; OnPropertyChanged(); } }
        public string DiscountText { get => _discountText; set { _discountText = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value ?? string.Empty;
                OnPropertyChanged();
                FilterMenu();
            }
        }

        // Количество добавляемой в заказ позиции.
        public decimal? Quantity
        {
            get => _quantity;
            set
            {
                var normalized = value is null ? 1m : Math.Clamp(value.Value, 0m, 99m);
                if (_quantity == normalized) return;
                _quantity = normalized;
                OnPropertyChanged();
            }
        }

        // Карта постоянного клиента, по которой рассчитывается скидка.
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (ReferenceEquals(_selectedClient, value)) return;
                _selectedClient = value;
                OnPropertyChanged();
                CalculateTotal();
            }
        }

        public int LastOrderId
        {
            get => _lastOrderId;
            private set
            {
                if (_lastOrderId == value) return;
                _lastOrderId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLastOrder));
                OnPropertyChanged(nameof(LastOrderText));
                _closeOrderCommand.RaiseCanExecuteChanged();
            }
        }

        public bool HasLastOrder => _lastOrderId > 0;

        public string LastOrderText => _lastOrderId > 0
            ? $"Заказ №{_lastOrderId} оплачен и ожидает закрытия"
            : string.Empty;

        public ICommand AddToCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand CloseOrderCommand => _closeOrderCommand;

        public MainViewModel(Employee user)
        {
            CurrentUser = user;
            AddToCartCommand = new RelayCommand<MenuItem>(AddToCart);
            IncreaseQuantityCommand = new RelayCommand<OrderItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<OrderItem>(DecreaseQuantity);
            RemoveFromCartCommand = new RelayCommand<OrderItem>(RemoveFromCart);
            ClearCartCommand = new RelayCommand(ClearCart);
            PayCommand = new AsyncRelayCommand(PayAsync);
            _closeOrderCommand = new RelayCommand(CloseLastOrder, () => HasLastOrder);

            LoadData();
        }

        // Загрузка доступного меню и карт постоянных клиентов.
        private void LoadData()
        {
            try
            {
                using var db = new AppDbContext();
                if (db.Database.CanConnect())
                {
                    DbStatus = "БД: ПОДКЛЮЧЕНО";

                    // Очищаем список перед загрузкой, чтобы не было дублей
                    MenuItems.Clear();
                    _allMenuItems.Clear();

                    // Загружаем блюда из БД (в стоп-листе не показываем)
                    var itemsFromDb = db.MenuItems
                        .Include(m => m.Category)
                        .Where(m => m.IsAvailable)
                        .OrderBy(m => m.Category!.CategoryName)
                        .ThenBy(m => m.ItemName)
                        .ToList();

                    foreach (var item in itemsFromDb)
                    {
                        _allMenuItems.Add(item);
                    }

                    FilterMenu();

                    Clients.Clear();
                    var noCard = new Client
                    {
                        ClientId = 0,
                        LastName = "Без карты",
                        FirstName = string.Empty,
                        Phone = string.Empty
                    };
                    Clients.Add(noCard);

                    foreach (var client in db.Clients
                                 .Include(c => c.Discount)
                                 .OrderBy(c => c.LastName)
                                 .ThenBy(c => c.FirstName)
                                 .ToList())
                    {
                        Clients.Add(client);
                    }

                    SelectedClient = noCard;
                    StatusMessage = $"Загружено позиций меню: {_allMenuItems.Count}.";
                }
                else
                {
                    DbStatus = "БД: НЕТ СВЯЗИ";
                    StatusMessage = "Нет связи с базой данных. Проверьте строку подключения в appsettings.json.";
                }
            }
            catch (Exception ex)
            {
                DbStatus = "ОШИБКА: " + (ex.InnerException?.Message ?? ex.Message);
                StatusMessage = "Не удалось загрузить меню: " + GetErrorMessage(ex);
            }
        }

        // Повторная загрузка меню после работы формы управления меню.
        public void ReloadMenu()
        {
            try
            {
                var items = _menuService.GetAvailable();

                _allMenuItems.Clear();
                _allMenuItems.AddRange(items);
                FilterMenu();

                StatusMessage = $"Меню обновлено. Доступно позиций: {items.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка обновления меню: " + GetErrorMessage(ex);
            }
        }

        private void FilterMenu()
        {
            var term = _searchText.Trim();

            MenuItems.Clear();

            foreach (var item in _allMenuItems)
            {
                if (term.Length == 0 ||
                    item.ItemName.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                    (item.Category?.CategoryName.Contains(term, StringComparison.CurrentCultureIgnoreCase) ?? false))
                {
                    MenuItems.Add(item);
                }
            }
        }

        private void AddToCart(MenuItem? menuItem)
        {
            if (menuItem == null) return;

            var quantity = (int)(Quantity ?? 0);
            if (quantity <= 0)
            {
                StatusMessage = "Количество должно быть больше нуля.";
                return;
            }

            var existing = Cart.FirstOrDefault(c => c.MenuItemId == menuItem.MenuItemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                Cart.Add(new OrderItem
                {
                    MenuItemId = menuItem.MenuItemId,
                    MenuItem = menuItem,
                    Quantity = quantity,
                    PriceAtOrder = menuItem.Price
                });
            }

            CalculateTotal();
            StatusMessage = $"Добавлено в заказ: {menuItem.ItemName} × {quantity}.";
        }

        private void IncreaseQuantity(OrderItem? item)
        {
            if (item == null) return;

            item.Quantity++;
            CalculateTotal();
        }

        private void DecreaseQuantity(OrderItem? item)
        {
            if (item == null) return;

            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                Cart.Remove(item);
            }

            CalculateTotal();
        }

        private void RemoveFromCart(OrderItem? item)
        {
            if (item == null) return;

            Cart.Remove(item);
            CalculateTotal();
            StatusMessage = $"Удалено из заказа: {item.ItemName}.";
        }

        private void ClearCart()
        {
            Cart.Clear();
            CalculateTotal();
            StatusMessage = "Заказ очищен.";
        }

        // Расчёт суммы заказа: базовая сумма позиций и итог с учётом скидки
        // постоянного клиента (5 % при сумме от 500 руб., 10 % при сумме от 1000 руб.).
        private void CalculateTotal()
        {
            SubTotal = Cart.Sum(c => c.Quantity * c.PriceAtOrder);

            var discount = SelectedClient?.Discount;
            var percent = discount?.DiscountPercent ?? 0m;
            var minOrderSum = discount?.MinOrderSum ?? 0m;

            if (_discountService.IsDiscountApplied(SubTotal, percent, minOrderSum))
            {
                TotalSum = _discountService.CalculateTotal(SubTotal, percent, minOrderSum);
                DiscountAmount = _discountService.CalculateDiscountAmount(SubTotal, percent, minOrderSum);
                DiscountText = $"Скидка {percent:0.##}%: −{DiscountAmount:N2} руб.";
            }
            else
            {
                TotalSum = _discountService.CalculateTotal(SubTotal, 0m, 0m);
                DiscountAmount = 0m;
                DiscountText = percent > 0
                    ? $"Скидка {percent:0.##}% от {minOrderSum:N2} руб. не применяется"
                    : "Скидка не применяется";
            }
        }

        // Оплата заказа: сохранение заказа в БД со статусом «оплачен».
        private async Task PayAsync()
        {
            if (Cart.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы одну позицию в заказ.";
                return;
            }

            try
            {
                var items = Cart.ToList();
                var total = TotalSum;
                int? clientId = SelectedClient is { ClientId: > 0 } client ? client.ClientId : null;

                var order = await Task.Run(() =>
                    _orderService.CreateOrder(CurrentUser.EmployeeId, clientId, items, total, "оплачен"));

                LastOrderId = order.OrderId;
                StatusMessage = $"Заказ №{order.OrderId} оплачен. Сумма: {order.TotalSum:N2} руб.";

                Cart.Clear();
                CalculateTotal();
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка оформления заказа: " + GetErrorMessage(ex);
            }
        }

        // Закрытие оплаченного заказа (перевод в статус «выполнен»).
        private void CloseLastOrder()
        {
            if (_lastOrderId <= 0) return;

            try
            {
                _orderService.ChangeStatus(_lastOrderId, "выполнен");
                StatusMessage = $"Заказ №{_lastOrderId} закрыт (выполнен).";
                LastOrderId = 0;
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка закрытия заказа: " + GetErrorMessage(ex);
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Простая реализация команды для MVVM
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute) => _execute = execute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public event EventHandler? CanExecuteChanged;
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute) => _execute = execute;

        public RelayCommand(Action execute, Func<bool>? canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public event EventHandler? CanExecuteChanged;
    }
}
