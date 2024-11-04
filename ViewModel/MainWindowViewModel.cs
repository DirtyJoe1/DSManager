using DSManager.Model;
using DSManager.Resources.Services;
using DSManager.View;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace DSManager.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields
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
        private Visibility datePickerVisibility = Visibility.Visible;
        public Visibility DatePickerVisibility { get => datePickerVisibility; set => Set(ref datePickerVisibility, value); }
        private Visibility fIOFilterVisibility = Visibility.Collapsed;
        public Visibility FIOFilterVisibility { get => fIOFilterVisibility; set => Set(ref fIOFilterVisibility, value); }
        private Visibility departmentsFilterVisibility = Visibility.Collapsed;
        public Visibility DepartmentsFilterVisibility { get => departmentsFilterVisibility; set => Set(ref departmentsFilterVisibility, value); }
        private bool isUndoEnabled = false;
        public bool IsUndoEnabled { get => isUndoEnabled; set => Set(ref isUndoEnabled, value); }
        private bool isRedoEnabled = false;
        public bool IsRedoEnabled { get => isRedoEnabled; set => Set(ref isRedoEnabled, value); }
        #endregion
        #region Commands
        public ICommand SaveDataGridCommand { get; }
        public ICommand OpenAddNewEntryWindowCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ImportDataCommand { get; }
        public ICommand DeleteRowCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand FilterCommand {  get; }
        #endregion

        public MainWindowViewModel(IWindowService windowService)
        {
            Application.Current.Dispatcher.Invoke(async () => await InitializeTable());
            _windowService = windowService;
            OpenAddNewEntryWindowCommand = new RelayCommand(OpenAddNewEntryWindow);
            var addNewEntryVM = new AddNewEntryWindowViewModel(this);
            HistoryManager.UndoRedoStateChanged += UpdateUndoRedoState;
            UpdateUndoRedoState();
            addNewEntryVM.EntryCreated += newEntry =>
            {
                Entries.Add(newEntry);
            };
            SaveDataGridCommand = new RelayCommand(SaveDataGrid);
            ImportDataCommand = new RelayCommand(ImportData);
            ExportDataCommand = new RelayCommand(ExportData);
            DeleteRowCommand = new RelayCommand(DeleteRow);
            UndoCommand = new RelayCommand(HistoryManager.Undo);
            RedoCommand = new RelayCommand(HistoryManager.Redo);
            FilterCommand = new RelayCommand(Filter);
        }
        private void OpenAddNewEntryWindow()
        {
            _windowService.ShowWindow<AddNewEntryWindow>(new AddNewEntryWindowViewModel(this));
        }

        public async Task InitializeTable()
        {
            Departments = new ObservableCollection<string>(ExcelService.ReadDepartments());
            await foreach (var entry in ExcelService.ReadExcelFile(ExcelService.ExcelFilePath))
            {
                Entries.Add(entry);
            }
        }
        public async Task RefreshTable()
        {
            var newEntries = await Task.Run(() => ExcelService.ReadExcelFile(ExcelService.ExcelFilePath));
            await CollectionService.ReplaceItemsInCollectionAsync(Entries, newEntries);
        }
        public void RefreshDepartments()
        {
            var newDepartments = ExcelService.ReadDepartments();
            CollectionService.ReplaceItemsInCollection(Departments, newDepartments);
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
        public async void ImportData()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл для импорта данных",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dialog.ShowDialog() == true)
            {
                await ExcelService.AppendDataFromExcel(dialog.FileName, ExcelService.ExcelFilePath);
                var newEntries = await Task.Run(() => ExcelService.ReadExcelFile(ExcelService.ExcelFilePath));
                await CollectionService.ReplaceItemsInCollectionAsync(Entries, newEntries);
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
        //Пофиксить этот бред
        public async void DeleteRow()
        {
            if (SelectedItem != null)
            {
                var newEntry = new DataModel
                {
                    Id = SelectedItem.Id,
                    FIO = SelectedItem.FIO,
                    Department = SelectedItem.Department,
                    Setup = SelectedItem.Setup,
                    Start = SelectedItem.Start,
                    End = SelectedItem.End,
                    Status = SelectedItem.Status
                };
                HistoryManager.Execute(new DeleteAction(newEntry, Entries));
                ExcelService.DeleteRowFromExcelFile(SelectedItem.Id, 0);
                await RefreshTable();
            }
        }
        public void Filter()
        {
            if(DatePickerVisibility == Visibility.Visible)
            {
                DatePickerVisibility = Visibility.Collapsed;
                FIOFilterVisibility = Visibility.Visible;
            }
            else if (FIOFilterVisibility == Visibility.Visible)
            {
                FIOFilterVisibility = Visibility.Collapsed;
                DepartmentsFilterVisibility = Visibility.Visible;
            }
            else
            {
                DepartmentsFilterVisibility= Visibility.Collapsed;
                DatePickerVisibility= Visibility.Visible;
            }
        }
        private void UpdateUndoRedoState()
        {
            IsUndoEnabled = HistoryManager.CanUndo;
            IsRedoEnabled = HistoryManager.CanRedo;
        }
    }
}
