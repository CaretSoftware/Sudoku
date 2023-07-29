using UnityEngine;

namespace Command {
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
