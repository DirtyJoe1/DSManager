using DSManager.Model;
using DSManager.Resources.Services;
using DSManager.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowService windowService = new();
            DataContext = new MainWindowViewModel(windowService);
        }
        //Пофиксить утечку памяти
        private async void Table_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            string propertyName = e.Column.SortMemberPath;
            var view = CollectionViewSource.GetDefaultView(Table.ItemsSource);
            if (view == null)
                return;
            ListSortDirection direction = e.Column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            e.Column.SortDirection = direction;
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            var data = Table.ItemsSource as List<DataModel>;
            if (data != null)
            {
                var sortedData = await Task.Run(() =>
                {
                    return direction == ListSortDirection.Ascending
                        ? data.OrderBy(x => x.GetType().GetProperty(propertyName).GetValue(x)).ToList()
                        : data.OrderByDescending(x => x.GetType().GetProperty(propertyName).GetValue(x)).ToList();
                });
                Table.ItemsSource = new ObservableCollection<DataModel>(sortedData);
            }
        }
    }
}
