using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DSManager.Resources.Services
{
    public class DateBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not DateTime endDate)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            DateTime now = DateTime.Now;
            TimeSpan diff = endDate - now;
            if (diff.TotalDays <= 7)
                return new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));
            else if(diff.TotalDays > 7 && diff.TotalDays <= 14)
                return new SolidColorBrush(Color.FromArgb(150, 255, 165, 0));
            else if (diff.TotalDays > 14 && diff.TotalDays <= 30)
                return new SolidColorBrush(Color.FromArgb(150, 255, 255, 0));
            else
                return new SolidColorBrush(Color.FromArgb(150, 0, 255, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
