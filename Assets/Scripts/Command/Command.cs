using System.Collections.Generic;

namespace Command {
    public abstract class Command {
    
        public abstract void Execute();
        
        public abstract void Undo();
    }
    
    public class FillNumber : Command {
        public delegate void AddTileChange(TileChange numberTile);
        private AddTileChange _addTileChange;
        private readonly int _index;
        private readonly (int from, int to) _numbers;
        private readonly List<TileChange> _tileChanges = new List<TileChange>();
        
        public FillNumber(int cellManagerIndex, int from, int to) {
            _addTileChange += AddTileChanges;
            _index = cellManagerIndex;
            _numbers.from = from;
            _numbers.to = to;
        }

        public override void Execute() {
            SudokuManager.SetNumber(_index, _numbers.to);
            for (int tile = 0; tile < _tileChanges.Count; tile++)
                _tileChanges[tile].Execute();
        }

        public override void Undo() {
            SudokuManager.SetNumber(_index, _numbers.from);
            for (int tile = 0; tile < _tileChanges.Count; tile++)
                _tileChanges[tile].Undo();
        }

        private void AddTileChanges(TileChange tileChange) => 
            _tileChanges.Add(tileChange);

        public AddTileChange AddTileChangeDelegate() => _addTileChange;

        ~FillNumber() => _addTileChange -= AddTileChanges;
    }
    
    public class TileChange : Command {
        private readonly NumberTile _numberTile;
        private readonly (bool prevValid, bool currValid) _values;
        
        public TileChange(NumberTile numberTile, bool prevValid, bool currValid) {
            _numberTile = numberTile;
            _values.prevValid = prevValid;
            _values.currValid = currValid;
        }

        public override void Execute() {
            _numberTile.Valid = _values.currValid;
        }

        public override void Undo() =>
            _numberTile.Valid = _values.prevValid;
    }
}
