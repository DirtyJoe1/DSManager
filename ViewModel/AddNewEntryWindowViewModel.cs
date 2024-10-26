using DSManager.Model;
using DSManager.Resources.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DSManager.ViewModel
{
    public class AddNewEntryWindowViewModel : ViewModelBase
    {
        #region Fields
        private string _fioField;
        public string FioField
        {
            get => _fioField;
            set
            {
                _fioField = value;
                OnPropertyChanged(nameof(FioField));
            }
        }
        private ObservableCollection<string> _departments = new();
        public ObservableCollection<string> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged(nameof(Departments));
            }
        }
        private string _selectedDepartment;
        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged(nameof(SelectedDepartment));
            }
        }
        private string _addDepartmentField;
        public string AddDepartmentField
        {
            get => _addDepartmentField;
            set
            {
                _addDepartmentField = value;
                OnPropertyChanged(nameof(AddDepartmentField));
            }
        }
        private string _deleteDepartmentField;
        public string DeleteDepartmentField
        {
            get => _deleteDepartmentField;
            set
            {
                _deleteDepartmentField = value;
                OnPropertyChanged(nameof(DeleteDepartmentField));
            }
        }
        private DateTime _start = DateTime.Today;
        public DateTime Start
        {
            get => _start;
            set
            {
                _start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
        private DateTime _setup = DateTime.Today;
        public DateTime Setup
        {
            get => _setup;
            set
            {
                _setup = value;
                OnPropertyChanged(nameof(Setup));
            }
        }
        private DateTime _end = DateTime.Today;
        public DateTime End
        {
            get => _end;
            set
            {
                _end = value;
                OnPropertyChanged(nameof(End));
            }
        }
        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }
        #endregion
        public ICommand AddDepartmentCommand { get; }
        public ICommand DeleteDepartmentCommand { get; }
        public ICommand CreateEntryCommand { get; }
        private MainWindowViewModel _mainWindowViewModel;
        public AddNewEntryWindowViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            UpdateFields();
            AddDepartmentCommand = new RelayCommand(AddDepartment);
            DeleteDepartmentCommand = new RelayCommand(DeleteDepartment);
            CreateEntryCommand = new RelayCommand(CreateEntry);
        }

        private void CreateEntry()
        {
            ExcelService.AddRow(FioField, SelectedDepartment, Setup, Start, End, SelectedStatus);
            _mainWindowViewModel.Entries.Add(new DataModel
            {
                Id = _mainWindowViewModel.Entries.Count + 1,
                FIO = FioField,
                Department = SelectedDepartment,
                Setup = Setup,
                Start = Start,
                End = End,
                Status = (Statuses)Enum.Parse(typeof(Statuses), SelectedStatus)
            });
        }

        private void AddDepartment()
        {
            if (string.IsNullOrEmpty(AddDepartmentField))
            {
                MessageBox.Show("Поле - пустое");
                return;
            }
            ExcelService.AddDepartment(AddDepartmentField);
            UpdateFields();
        }
        private void UpdateFields()
        {
            Departments.Clear();
            var deps = new ObservableCollection<string>(ExcelService.ReadDepartments());
            foreach (var department in deps)
            {
                Departments.Add(department);
            }
        }

        private void DeleteDepartment()
        {
            if (DeleteDepartmentField == SelectedDepartment)
            {
                SelectedDepartment = "";
            }
            //IndexOf full rewrite possible solution
            ExcelService.DeleteRowFromExcelFile(Departments.IndexOf(DeleteDepartmentField), 1);
            UpdateFields();
        }
    }
}
