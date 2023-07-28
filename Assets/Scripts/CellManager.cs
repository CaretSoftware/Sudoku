using UnityEngine;
using TMPro;

public class CellManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private GameObject blankTilePrefab;
    [SerializeField] private Transform numberTileParent;

    private static int _size;
    private static int _staticIndex;
    private readonly NumberTile[] _numberTiles = new NumberTile[_size];
    
    public int Index { get; private set; }

    private void Awake() => Index = _staticIndex++;

    private void Start() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(Index);

        if (number == Sudoku.Blank)
            SpawnTiles();
        else
            text.text = number.ToString();
    }

    private void SpawnTiles() {
        for (int i = 1; i <= _size; i++) {
            NumberTile numberTile = 
                Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
            numberTile.MyCellManager = this;
            numberTile.Number = i;
            _numberTiles[i - 1] = numberTile;
            numberTile.Valid(Sudoku.Valid(Index, i));
        }
    }

    public void ClickedTile(int number) {
        Command.Processor.ExecuteCommand(
                new Command.FillNumber(this, Sudoku.Number(Index), number));
        //SetNumber(number);
        //SudokuManager.SetNumber(Index, number);
        //ClearAllTiles();
    }

    public void SetNumber(int number) {
        if (number == Sudoku.Blank) {
            SetNumber(string.Empty);
            ShowAllTiles();
        } else {
            SetNumber(number.ToString());
            ClearAllTiles();
        }
    }

    private void ShowAllTiles() {
        for (int tile = 0; tile < _numberTiles.Length; tile++) {
            
        }
    }

    private void SetNumber(string s) => text.text = s;

    private void ClearAllTiles() {
        for (int number = 1; number <= _size; number++)
            ClearTile(number, assigned: true);
    }

    public void ClearTiles(bool valid, params int[] numbers) {
        for (int i = 0; i < numbers.Length; i++)
            ClearTile(numbers[i], valid);
    }
    
    private void ClearTile(int number, bool valid = true, bool assigned = false) {
        if (number - 1 < 0 || number - 1 >= _numberTiles.Length)
            Debug.Log($"num: {number}");
        NumberTile tile = _numberTiles[number - 1];
        if (tile != null && !tile.Cleared)
            tile.Clear(clear: true, valid, assigned);
    }

    public static void ResetCells(int size) {
        _staticIndex = 0;
        _size = size;
    }
}
