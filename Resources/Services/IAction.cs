using DSManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Resources.Services
{
    //Проблемный класс, который реализует действия отмены и возврата элементов
    //Когда добавляешь новый элемент, вроде все прекрасно работает, потому что используется AddAction
    //А вот с DeleteAction, какая-то беда, при возврате элемента, его индекс и индекс ближайшего элемента становится одинаковым
    //Я так и не смог это пофиксить, может у тебя получится, а я просто тупил :)
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
        private readonly int _originalIndex;

        public DeleteAction(DataModel data, ObservableCollection<DataModel> collection)
        {
            _data = data;
            _collection = collection;
            _originalIndex = data.Id;
        }
        //Пофиксить эту шляпу
        public void Undo()
        {
            if (_originalIndex >= 0 && _originalIndex < _collection.Count)
            {
                _collection.Insert(_originalIndex, _data);
                ExcelService.InsertRow(_originalIndex, _data.FIO, _data.Department, _data.Setup, _data.Start, _data.End, _data.Status);
            }
            else
            {
                _collection.Add(_data);
                ExcelService.AddRow(_data.FIO, _data.Department, _data.Setup, _data.Start, _data.End, _data.Status);
            }
        }
        //Или может эту, если задуматься, то скорее всего эту
        public void Redo()
        {
            if (_collection.Contains(_data))
            {
                _collection.Remove(_data);
                ExcelService.DeleteRowFromExcelFile(_data.Id, 0);
            }
        }
    }
}
