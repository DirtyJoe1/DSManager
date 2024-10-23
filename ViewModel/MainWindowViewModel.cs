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
        private readonly IWindowService _windowService;
        private ObservableCollection<DataModel> _entries;
        public ObservableCollection<DataModel> Entries
        {
            get => _entries;
            set
            {
                _entries = value;
                OnPropertyChanged(nameof(Entries));
            }
        }
        private ObservableCollection<string> _departments;
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

        public MainWindowViewModel(IWindowService windowService)
        {
            UpdateTable();
            _windowService = windowService;
            OpenAddNewEntryWindowCommand = new RelayCommand(() => windowService.ShowWindow<AddNewEntryWindow>(new AddNewEntryWindowViewModel(this)));
            SaveDataGridCommand = new RelayCommand(SaveDataGrid);
            ImportDataCommand = new RelayCommand(ImportData);
            ExportDataCommand = new RelayCommand(ExportData);
        }
        public void UpdateTable()
        {
            Departments = new ObservableCollection<string>(ExcelService.ReadDepartments(Repository.ExcelFilePath));
            Entries = new ObservableCollection<DataModel>(ExcelService.ReadExcelFile(Repository.ExcelFilePath));
        }
        public void SaveDataGrid()
        {
            try
            {
                ExcelService.SaveDataGrid(Entries, Repository.ExcelFilePath, false);
                UpdateTable();
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
                ExcelService.AppendDataFromExcel(dialog.FileName, Repository.ExcelFilePath);
                Entries.Clear();
                var newEntries = ExcelService.ReadExcelFile(Repository.ExcelFilePath);
                foreach (var item in newEntries)
                {
                    Entries.Add(item);
                }
            }
        }
    }
}
