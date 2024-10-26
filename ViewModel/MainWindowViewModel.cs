using DSManager.Model;
using DSManager.Resources.Services;
using DSManager.View;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DSManager.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public IWindowService _windowService;
        private ObservableCollection<DataModel> _entries = new();
        public ObservableCollection<DataModel> Entries
        {
            get => _entries;
            set
            {
                _entries = value;
                OnPropertyChanged(nameof(Entries));
            }
        }
        private DataModel _selectedItem;
        public DataModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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
        public ICommand SaveDataGridCommand { get; }
        public ICommand OpenAddNewEntryWindowCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ImportDataCommand { get; }
        public ICommand DeleteRowCommand { get; }

        public MainWindowViewModel(IWindowService windowService)
        {
            InitializeTable();
            _windowService = windowService;
            OpenAddNewEntryWindowCommand = new RelayCommand(OpenAddNewEntryWindow);
            SaveDataGridCommand = new RelayCommand(SaveDataGrid);
            ImportDataCommand = new RelayCommand(ImportData);
            ExportDataCommand = new RelayCommand(ExportData);
            DeleteRowCommand = new RelayCommand(DeleteRow);
        }
        private void OpenAddNewEntryWindow()
        {
            _windowService.ShowWindow<AddNewEntryWindow>(new AddNewEntryWindowViewModel(this));
        }
        private void DeleteRow()
        {
            if (SelectedItem != null)
            {
                ExcelService.DeleteRowFromExcelFile(Entries.IndexOf(SelectedItem) + 1, 0);
                UpdateTable();
            }
        }
        public void InitializeTable()
        {
            Departments = new ObservableCollection<string>(ExcelService.ReadDepartments());
            Entries = new ObservableCollection<DataModel>(ExcelService.ReadExcelFile(ExcelService.ExcelFilePath));
        }
        public void UpdateTable()
        {
            Entries.Clear();
            var newEntries = ExcelService.ReadExcelFile(ExcelService.ExcelFilePath);
            foreach (var item in newEntries)
            {
                Entries.Add(item);
            }
        }
        public void SaveDataGrid()
        {
            try
            {
                ExcelService.SaveDataGrid(Entries, false);
                MessageBox.Show("Успешно сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void ExportData()
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SelectedPath;
                if (selectedPath != null)
                {
                    ExcelService.ExportFile(selectedPath);
                }
            }
        }
        public void ImportData()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл для импорта данных",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dialog.ShowDialog() == true)
            {
                ExcelService.AppendDataFromExcel(dialog.FileName, ExcelService.ExcelFilePath);
                Entries.Clear();
                var newEntries = ExcelService.ReadExcelFile(ExcelService.ExcelFilePath);
                foreach (var item in newEntries)
                {
                    Entries.Add(item);
                }
            }
        }
    }
}
