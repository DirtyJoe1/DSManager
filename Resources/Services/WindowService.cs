using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DSManager.Resources.Services
{
    public class WindowService : IWindowService
    {
        //Класс, который реализуется из интерфейса IWindowService, суть простая, показать окно и установить
        //в качестве контекста данных ViewModel, который передается в конструктор.
        //По факту, наверное, можно переделать в статический, но пока так
        public void ShowWindow<T>(object viewModel) where T : Window, new()
        {
            var window = new T
            {
                DataContext = viewModel
            };
            window.Show();
        }
    }
}
