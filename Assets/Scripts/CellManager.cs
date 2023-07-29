using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private Transform numberTileParent;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;

    private static int _size;
    private static int _staticIndex;
    private readonly NumberTile[] _numberTiles = new NumberTile[_size];
    private int _index;

    private void Awake() => _index = _staticIndex++;

    private void Start() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(_index);
        bool cleared = number != Sudoku.Blank;
        SpawnTiles(cleared);
        text.text = cleared ? number.ToString() : string.Empty;
    }

    private void SpawnTiles(bool cleared) {
        
        float numberTileDimension = sudokuPanelDimension / _size / Mathf.Sqrt(_size);
        Vector2 numberTileSize = new Vector2(numberTileDimension, numberTileDimension);
        gridLayout.cellSize = numberTileSize;

        for (int i = 1; i <= _size; i++) {
            NumberTile numberTile = 
                Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
            numberTile.MyCellManager = this;
            numberTile.Number = i;
            numberTile.Valid = Sudoku.Valid(_index, i);
            numberTile.Cleared = cleared;
            _numberTiles[i - 1] = numberTile;
            numberTile.SetSize(numberTileSize);
        }
    }

    public void ClickedTile(int number) {
        SudokuManager.FillNumber(_index, number);
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
