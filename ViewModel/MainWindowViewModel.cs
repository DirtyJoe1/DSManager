using DSManager.Model;
using DSManager.Resources.Services;
using DSManager.View;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace DSManager.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        //Если нужно работать с коммандами или полями, то расширь регионы Fields и/или Commands, их тупо очень много поэтому
        //я пихнул все в регион, чтобы скрывать когда не нужно
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
        private ObservableCollection<DataModel> _entriesTemp = new();
        public ObservableCollection<DataModel> EntriesTemp
        {
            get => _entriesTemp;
            set
            {
                _entries = value;
                OnPropertyChanged(nameof(EntriesTemp));
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
        private DataModel _selectedItem;
        public DataModel SelectedItem { get => _selectedItem; set =>  Set(ref _selectedItem, value); }
        private Visibility datePickerVisibility = Visibility.Visible;
        public Visibility DatePickerVisibility { get => datePickerVisibility; set => Set(ref datePickerVisibility, value); }
        private Visibility fIOFilterVisibility = Visibility.Collapsed;
        public Visibility FIOFilterVisibility { get => fIOFilterVisibility; set => Set(ref fIOFilterVisibility, value); }
        private Visibility departmentsFilterVisibility = Visibility.Collapsed;
        public Visibility DepartmentsFilterVisibility
        {
            get => departmentsFilterVisibility; set => Set(ref departmentsFilterVisibility, value);
        }
        private bool isUndoEnabled = false;
        public bool IsUndoEnabled { get => isUndoEnabled; set => Set(ref isUndoEnabled, value); }
        private bool isRedoEnabled = false;
        public bool IsRedoEnabled { get => isRedoEnabled; set => Set(ref isRedoEnabled, value); }
        private string fioFilter;
        public string FioFilter
        {
            get => fioFilter;
            set 
            {
                fioFilter = value;
                OnPropertyChanged(nameof(FioFilter));
                FioFiltration();
            } 
        }
        private DateTime? _startDateFilter;
        public DateTime? StartDateFilter
        {
            get => _startDateFilter;
            set
            {
                _startDateFilter = value;
                OnPropertyChanged(nameof(StartDateFilter));
                DateFiltration();
            }
        }

        private DateTime? _endDateFilter;
        public DateTime? EndDateFilter
        {
            get => _endDateFilter;
            set
            {
                _endDateFilter = value;
                OnPropertyChanged(nameof(EndDateFilter));
                DateFiltration();
            }
        }
        private string _departmentFilter;
        public string DepartmentFilter
        {
            get => _departmentFilter;
            set
            {
                _departmentFilter = value;
                OnPropertyChanged(nameof(DepartmentFilter));
                DepartmentFiltration();
            }
        }
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
        public ICommand ClearFilterCommand { get; }
        public ICommand ExitCommand { get; }
        #endregion

        public MainWindowViewModel(IWindowService windowService)
        {
            //Инициализация всей шляпы, работает не трогай
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
            SaveDataGridCommand = new RelayCommand(() => SaveDataGrid());
            ImportDataCommand = new RelayCommand(ImportData);
            ExportDataCommand = new RelayCommand(ExportData);
            DeleteRowCommand = new RelayCommand(DeleteRow);
            UndoCommand = new RelayCommand(HistoryManager.Undo);
            RedoCommand = new RelayCommand(HistoryManager.Redo);
            FilterCommand = new RelayCommand(Filter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            ExitCommand = new RelayCommand(() => Exit());
        }
        //Открывает окно добавления записи и присвавает ему ссылку на этот ViewModel, а также устанавливает свой нужный VM
        public void OpenAddNewEntryWindow()
        {
            _windowService.ShowWindow<AddNewEntryWindow>(new AddNewEntryWindowViewModel(this));
        }
        //Первичная инициализация DataGrid, вызывается 1 раз при запуске проги
        public async Task InitializeTable()
        {
            Departments = new ObservableCollection<string>(ExcelService.ReadDepartments());
            await foreach (var entry in ExcelService.ReadExcelFile(ExcelService.ExcelFilePath))
            {
                Entries.Add(entry);
            }
            CollectionService.ReplaceItemsInCollection(EntriesTemp, Entries);
        }
        //Снизу два метода вызывают обновление своих полей
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
        //
        
        //Тут просто, экспорт
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
        //А тут импорт
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
        //Сохранение DataGrid'а, тут как раз можно установить backup, если надо
        public void SaveDataGrid(bool backup = false)
        {
            try
            {
                ExcelService.SaveDataGrid(Entries, backup);
                MessageBox.Show("Успешно сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //Есть класс HistoryManager, он нужен для работы с отменой и возвратом (подробнее в самом классе)
        //Пофиксить этот бред, что-то в HistoryManager'е, а может в DeleteAction делает так,
        //что при удалении, а потом возвращении записи, ближайший элемент до становится с таким же индексом 
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
        //Если удалить или добавить запись при фильтрации, она не отобразится после закрытия фильтра
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
        //Очищение фильтров под капотом работает, а вот визуал, что-то не обновляется, тут хз :/
        public void ClearFilter()
        {
            if (DatePickerVisibility == Visibility.Visible)
            {
                StartDateFilter = null;
                EndDateFilter = null;
            }
            else if (FIOFilterVisibility == Visibility.Visible)
            {
                FioFilter = "";
            }
            else
            {
                DepartmentFilter = "";
            }
            CollectionService.ReplaceItemsInCollection(Entries, EntriesTemp);
        }
        //Тут фильтрация по ФИО
        public void FioFiltration()
        {
            if (string.IsNullOrEmpty(FioFilter))
            {
                CollectionService.ReplaceItemsInCollection(Entries, EntriesTemp);
            }
            else
            {
                var filteredEntries = EntriesTemp
                    .Where(entry => entry.FIO != null && entry.FIO.Contains(FioFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                CollectionService.ReplaceItemsInCollection(Entries, filteredEntries);
            }
        }
        //Тут по датам
        public void DateFiltration()
        {
            var filteredEntries = EntriesTemp.AsEnumerable();
            if (StartDateFilter.HasValue)
            {
                filteredEntries = filteredEntries
                    .Where(entry => entry.Start.HasValue && entry.Start.Value >= StartDateFilter.Value);
            }
            if (EndDateFilter.HasValue)
            {
                filteredEntries = filteredEntries
                    .Where(entry => entry.End.HasValue && entry.End.Value <= EndDateFilter.Value);
            }
            CollectionService.ReplaceItemsInCollection(Entries, filteredEntries);
        }
        //Тут по отделению/подразделению
        private void DepartmentFiltration()
        {
            var filteredEntries = EntriesTemp.AsEnumerable();
            if (!string.IsNullOrEmpty(DepartmentFilter) && DepartmentsFilterVisibility == Visibility.Visible)
            {
                filteredEntries = filteredEntries
                    .Where(entry => entry.Department != null && entry.Department.Contains(DepartmentFilter, StringComparison.OrdinalIgnoreCase));
                CollectionService.ReplaceItemsInCollection(Entries, filteredEntries);
            }
            else
            {
                CollectionService.ReplaceItemsInCollection(Entries, EntriesTemp);
            }
        }
        // Инициализация отмены и возврата
        public void UpdateUndoRedoState()
        {
            IsUndoEnabled = HistoryManager.CanUndo;
            IsRedoEnabled = HistoryManager.CanRedo;
        }

        //Окошко при выходе, три варианта,
        //если да - то сохраняет и делает резервную копию,
        //если нет - то просто выходит,
        //если отмена - то закрывает окошко и не выходит из проги
        public bool Exit()
        {
            var result = MessageBox.Show("Сохранять ли изменения в файл?", "Внимание!", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveDataGrid(true);
                    return true;
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                    return false;
            }
            return true;
        }
    }
}
