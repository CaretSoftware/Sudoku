using System.Collections.Generic;

namespace Command {
    public abstract class CompoundCommand : Command {
        private List<Command> _commands;

        public CompoundCommand(List<Command> commands) => _commands = commands;

        public void AddCommand(Command command) => _commands.Add(command);

        public void AddCommands(List<Command> commands) => _commands.AddRange(commands);

        public override void Execute() {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Execute();
        }

        public override void Undo() {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Undo();
        }
    }
}
