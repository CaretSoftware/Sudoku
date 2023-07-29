using UnityEngine;
using TMPro;

public class CellManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private Transform numberTileParent;

    private static int _size;
    private static int _staticIndex;
    private readonly NumberTile[] _numberTiles = new NumberTile[_size];
    
    public int Index { get; private set; }

    private void Awake() => Index = _staticIndex++;

    private void Start() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(Index);
        bool cleared = number != Sudoku.Blank;
        SpawnTiles(cleared);
        text.text = cleared ? number.ToString() : string.Empty;
    }

    private void SpawnTiles(bool cleared) {
        for (int i = 1; i <= _size; i++) {
            NumberTile numberTile = 
                Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
            numberTile.MyCellManager = this;
            numberTile.Number = i;
            _numberTiles[i - 1] = numberTile;
            numberTile.Valid = Sudoku.Valid(Index, i);
            numberTile.Cleared = cleared;
        }
    }

    public void ClickedTile(int number) {
        SudokuManager.FillNumber(Index, number);
    }

    public void SetNumber(int number) {
        if (number != Sudoku.Blank) {
            SetNumber(number.ToString());
            ClearAllTiles(clear: true);
        } else {
            SetNumber(string.Empty);
            ClearAllTiles(clear: false);
        }
    }

    private void SetNumber(string s) => text.text = s;

    private void ClearAllTiles(bool clear) {
        for (int number = 1; number <= _size; number++)
            _numberTiles[number - 1].Cleared = clear;
    }

    public void ClearTiles(Command.FillNumber.AddTileChange addTileChange, params int[] numbers) {
        for (int i = 0; i < numbers.Length; i++)
            ClearTile(addTileChange, numbers[i]);
    }
    
    private void ClearTile(Command.FillNumber.AddTileChange addTileChange, int number) {
        NumberTile tile = _numberTiles[number - 1];
        tile.Clear(addTileChange);
    }

    public static void ResetCells(int size) {
        _staticIndex = 0;
        _size = size;
    }
}
