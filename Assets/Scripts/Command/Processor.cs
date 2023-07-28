using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Command {
    public static class Processor {
        private static List<Command> _commands = new List<Command>();
        private static int _currentCommandIndex = -1;

        public static void ExecuteCommand(Command command) {
            // remove redos if not at end of _commands-list
            _commands.RemoveRange(
                    index: _currentCommandIndex + 1, 
                    count: _commands.Count - _currentCommandIndex - 1);
            command.Execute();
            _commands.Add(command);
            _currentCommandIndex++;
        }

        public static void Undo() {
            if (_currentCommandIndex < 0) 
                return;
            
            _commands[_currentCommandIndex].Undo();
            _currentCommandIndex--;
        }

        public static void Redo() {
            if (_currentCommandIndex >= _commands.Count - 1) 
                return;
            
            _currentCommandIndex++;
            _commands[_currentCommandIndex].Execute();
        }
    }
}
