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
            DateTime endDate = (DateTime)value;
            DateTime now = DateTime.Now;
            TimeSpan diff = endDate - now;
            if (diff.TotalDays <= 7)
                return new SolidColorBrush(Color.FromArgb(96, 255, 0, 0));
            else if(diff.TotalDays > 7 && diff.TotalDays <= 30)
                return new SolidColorBrush(Color.FromArgb(96, 0, 0, 255));
            else if (diff.TotalDays > 30 && diff.TotalDays <= 180)
                return new SolidColorBrush(Color.FromArgb(96, 255, 255, 0));
            else
                return new SolidColorBrush(Color.FromArgb(96, 0, 255, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
