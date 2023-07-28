using System.Collections.Generic;

namespace Command {
    public class FillNumber : Command {
        public delegate void AddToTileChangesList(TileChange numberTile);
        private AddToTileChangesList _addToTileChangesList;

        private readonly CellManager _cellmanager;
        private readonly (int from, int to) _values;
        private List<TileChange> _tileChanges = new List<TileChange>();
        
        public FillNumber(CellManager cellManager, int from, int to) {
            _addToTileChangesList += AddToTileList;
            _cellmanager = cellManager;
            _values.from = from;
            _values.to = to;
        }

        public override void Execute() {
            _cellmanager.SetNumber(_values.to);
            Sudoku.SetNumber(_cellmanager.Index, _values.to);
            //for (int tile = 0; tile < _tileChanges.Count; tile++)
            //    _tileChanges[tile].Execute();
        }

        public override void Undo() {
            _cellmanager.SetNumber(_values.from);
            Sudoku.SetNumber(_cellmanager.Index, _values.from);
            //for (int tile = 0; tile < _tileChanges.Count; tile++)
            //    _tileChanges[tile].Undo();
        }

        private void AddToTileList(TileChange tileChange) => _tileChanges.Add(tileChange);

        ~FillNumber() => _addToTileChangesList -= AddToTileList;
    }
}
