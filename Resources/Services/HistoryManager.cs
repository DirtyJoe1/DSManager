using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Resources.Services
{
    //Опять же весь класс описывать не хочется поэтому в кратце
    //История действий, записывается в два стака(я надеюсь ты помнишь, что это такое ;)   )
    //Основной функционал находится в интерфейсе IAction, за пояснениями туда
    public static class HistoryManager
    {
        public static event Action UndoRedoStateChanged;
        private static readonly Stack<IAction> _undoStack = new();
        private static readonly Stack<IAction> _redoStack = new();
        public static void Execute(IAction action)
        {
            action.Redo();
            _undoStack.Push(action);
            _redoStack.Clear();
            UndoRedoStateChanged?.Invoke();
        }

        public static void Undo()
        {
            if (CanUndo)
            {
                var action = _undoStack.Pop(); 
                action.Undo(); 
                _redoStack.Push(action);
                UndoRedoStateChanged?.Invoke();
            }
        }
        public static void Redo()
        {
            if (CanRedo)
            {
                var action = _redoStack.Pop(); 
                action.Redo();  
                _undoStack.Push(action);
                UndoRedoStateChanged?.Invoke();
            }
        }
        public static bool CanUndo => _undoStack.Count > 0;
        public static bool CanRedo => _redoStack.Count > 0;
    }
}
