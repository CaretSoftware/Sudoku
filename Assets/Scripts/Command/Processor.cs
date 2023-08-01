using System.Collections.Generic;

namespace Command {
    public static class Processor {
        public delegate void UndoDelegate(bool empty);
        public static UndoDelegate UndoEmptyDelegate;
        private static List<Command> _commands = new List<Command>();
        private static int _currentCommandIndex = -1;

        public static void ExecuteCommand(Command command) {
            _commands.RemoveRange( // remove redos if not at end of _commands-list
                index: _currentCommandIndex + 1,
                count: _commands.Count - _currentCommandIndex - 1);
            command.Execute();
            _commands.Add(command);
            _currentCommandIndex++;
            UndoEmptyDelegate.Invoke(empty: _currentCommandIndex < 0);
        }

        public static void Undo() {
            if (_currentCommandIndex < 0)
                return;

            _commands[_currentCommandIndex].Undo();
            _currentCommandIndex--;
            UndoEmptyDelegate.Invoke(empty: _currentCommandIndex < 0);
        }

        public static void Redo() {
            if (_currentCommandIndex >= _commands.Count - 1)
                return;

            _currentCommandIndex++;
            _commands[_currentCommandIndex].Execute();
            UndoEmptyDelegate.Invoke(empty: _currentCommandIndex < 0);
        }

        public static void ClearUndo() {
            _commands = new List<Command>();
            _currentCommandIndex = -1;
            UndoEmptyDelegate?.Invoke(empty: true);
        }
    }
}
