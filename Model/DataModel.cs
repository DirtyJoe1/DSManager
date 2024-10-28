using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Model
{
    public class DataModel : INotifyPropertyChanged
    {
        private int _id;
        public int Id 
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        private string _fio;
        public string FIO
        {
            get => _fio;
            set
            {
                _fio = value;
                OnPropertyChanged(nameof(FIO));
            }
        }
        private string _department;
        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
            }
        }
        private DateTime? _setup;
        public DateTime? Setup
        {
            get => _setup;
            set
            {
                _setup = value;
                OnPropertyChanged(nameof(Setup));
            }
        }
        private DateTime? _start;
        public DateTime? Start
        {
            get => _start;
            set
            {
                _start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
        private DateTime? _end;
        public DateTime? End
        {
            get => _end;
            set
            {
                _end = value;
                OnPropertyChanged(nameof(End));
            }
        }
        private Statuses _statuses;
        public Statuses Status
        {
            get => _statuses;
            set
            {
                _statuses = value;
                OnPropertyChanged(nameof(Statuses));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
