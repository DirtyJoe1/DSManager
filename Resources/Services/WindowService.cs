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
