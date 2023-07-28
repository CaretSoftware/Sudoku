using UnityEngine;

namespace Command {
    public class TileChange : Command {
        private readonly NumberTile _numberTile;
        private readonly (bool clear, bool valid, bool assigned) _values;
        
        public TileChange(NumberTile numberTile, bool valid) {
            _numberTile = numberTile;
            _values.valid = valid;
        }

        public override void Execute() =>
            _numberTile.Clear(_values.clear, _values.valid, _values.assigned);

        public override void Undo() =>
            _numberTile.Clear(!_values.clear, _values.valid, !_values.assigned);
    }
}
