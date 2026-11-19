using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using fastfoodshop.Models;
using fastfoodshop.Services;

namespace fastfoodshop.ViewModels
{
    // Управление учётными записями сотрудников и назначение ролей.
    public class UsersViewModel : ViewModelBase
    {
        private readonly UserService _userService = new();
        private readonly Employee _currentUser;

        private Employee? _selectedEmployee;
        private Role? _selectedRole;
        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _middleName = string.Empty;
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = "Выберите сотрудника или создайте нового.";

        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<Role> Roles { get; } = new();

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (!SetProperty(ref _selectedEmployee, value)) return;
                LoadFields();
            }
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string MiddleName
        {
            get => _middleName;
            set => SetProperty(ref _middleName, value);
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand NewCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public UsersViewModel(Employee currentUser)
        {
            _currentUser = currentUser;

            NewCommand = new RelayCommand(StartNew);
            AddCommand = new RelayCommand(Add);
            SaveCommand = new RelayCommand(Save);
            DeleteCommand = new RelayCommand(Delete);
            RefreshCommand = new RelayCommand(Load);

            Load();
        }

        public void Load()
        {
            try
            {
                var roles = _userService.GetRoles();
                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }

                var employees = _userService.GetAll();
                Employees.Clear();
                foreach (var employee in employees)
                {
                    Employees.Add(employee);
                }

                _selectedEmployee = null;
                OnPropertyChanged(nameof(SelectedEmployee));
                ClearFields();

                StatusMessage = $"Загружено сотрудников: {Employees.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки пользователей: " + GetErrorMessage(ex);
            }
        }

        private void LoadFields()
        {
            if (SelectedEmployee == null)
            {
                ClearFields();
                return;
            }

            LastName = SelectedEmployee.LastName;
            FirstName = SelectedEmployee.FirstName;
            MiddleName = SelectedEmployee.MiddleName ?? string.Empty;
            Login = SelectedEmployee.Login;
            Password = string.Empty;
            SelectedRole = Roles.FirstOrDefault(r => r.RoleId == SelectedEmployee.RoleId);
        }

        private void ClearFields()
        {
            LastName = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            Login = string.Empty;
            Password = string.Empty;
            SelectedRole = Roles.FirstOrDefault();
        }

        private void StartNew()
        {
            _selectedEmployee = null;
            OnPropertyChanged(nameof(SelectedEmployee));
            ClearFields();
            StatusMessage = "Заполните данные нового сотрудника и нажмите «Добавить».";
        }

        private void Add()
        {
            if (SelectedRole == null)
            {
                StatusMessage = "Выберите роль сотрудника.";
                return;
            }

            try
            {
                var created = _userService.Create(LastName, FirstName, MiddleName, Login, Password,
                    SelectedRole.RoleId);

                Load();
                SelectedEmployee = Employees.FirstOrDefault(e => e.EmployeeId == created.EmployeeId);
                StatusMessage = $"Сотрудник «{created.FullName}» добавлен.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка добавления: " + GetErrorMessage(ex);
            }
        }

        private void Save()
        {
            if (SelectedEmployee == null)
            {
                StatusMessage = "Сначала выберите сотрудника или нажмите «Добавить».";
                return;
            }

            if (SelectedRole == null)
            {
                StatusMessage = "Выберите роль сотрудника.";
                return;
            }

            try
            {
                var employeeId = SelectedEmployee.EmployeeId;

                _userService.Update(employeeId, LastName, FirstName, MiddleName, Login, Password,
                    SelectedRole.RoleId);

                Load();
                SelectedEmployee = Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                StatusMessage = "Изменения сохранены.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка сохранения: " + GetErrorMessage(ex);
            }
        }

        private void Delete()
        {
            if (SelectedEmployee == null)
            {
                StatusMessage = "Сначала выберите сотрудника.";
                return;
            }

            if (SelectedEmployee.EmployeeId == _currentUser.EmployeeId)
            {
                StatusMessage = "Нельзя удалить учётную запись, под которой выполнен вход.";
                return;
            }

            var name = SelectedEmployee.FullName;

            try
            {
                _userService.Delete(SelectedEmployee.EmployeeId);
                Load();
                StatusMessage = $"Сотрудник «{name}» удалён.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка удаления: " + GetErrorMessage(ex);
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
    }
}
