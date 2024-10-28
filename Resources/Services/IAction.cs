using DSManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Resources.Services
{
    public interface IAction
    {
        void Undo();
        void Redo();
    }
    public class AddAction : IAction
    {
        private readonly DataModel _data;
        private readonly ObservableCollection<DataModel> _collection;

        public AddAction(DataModel data, ObservableCollection<DataModel> collection)
        {
            _data = data;
            _collection = collection;
        }

        public void Undo()
        {
            _collection.Remove(_data);
            ExcelService.DeleteRowFromExcelFile(_data.Id, 0);
        }

        public void Redo()
        {
            _collection.Add(_data);
            ExcelService.AddRow(_data.FIO, _data.Department, _data.Setup, _data.Start, _data.End, _data.Status);
        }
    }

    public class DeleteAction : IAction
    {
        private readonly DataModel _data;
        private readonly ObservableCollection<DataModel> _collection;

        public DeleteAction(DataModel data, ObservableCollection<DataModel> collection)
        {
            _data = data;
            _collection = collection;
        }

        public void Undo()
        {
            _collection.Add(_data);
            ExcelService.DeleteRowFromExcelFile(_data.Id, 0);
        }

        public void Redo()
        {
            _collection.Remove(_data);
            ExcelService.AddRow(_data.FIO, _data.Department, _data.Setup, _data.Start, _data.End, _data.Status);
        }
    }
}
