using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Model
{
    public class DataModel : INotifyPropertyChanged
    {
        private string fio;
        public string FIO
        {
            get => fio;
            set
            {
                fio = value;
                OnPropertyChanged(nameof(fio));
            }
        }
        private string department;
        public string Department
        {
            get => department;
            set
            {
                department = value;
                OnPropertyChanged(nameof(department));
            }
        }
        private DateTime setup;
        public DateTime Setup
        {
            get => setup;
            set
            {
                setup = value;
                OnPropertyChanged(nameof(setup));
            }
        }
        private DateTime start;
        public DateTime Start
        {
            get => start;
            set
            {
                start = value;
                OnPropertyChanged(nameof(start));
            }
        }
        private DateTime end;
        public DateTime End
        {
            get => end;
            set
            {
                end = value;
                OnPropertyChanged(nameof(end));
            }
        }
        private Statuses status;
        public Statuses Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged(nameof(status));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
