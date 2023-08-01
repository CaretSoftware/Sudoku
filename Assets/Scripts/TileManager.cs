using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private Transform numberTileParent;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private Outline outline;
    [SerializeField] private float defaultOutlineDistance = 5f;

    private static int _staticIndex;
    private int _numberOfActiveTiles;
    private readonly List<NumberTile> _numberTiles = new List<NumberTile>();
    private int _index;

    private void Awake() => _index = _staticIndex++;

    private void OnEnable() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(_index);
        bool cleared = number != Sudoku.Blank;
        InitializeTiles(cleared);
        text.text = cleared ? number.ToString() : string.Empty;
    }

    public void InitializeTiles(bool cleared) {
        float numberTileDimension = sudokuPanelDimension / Sudoku.Size / Mathf.Sqrt(Sudoku.Size);
        Vector2 numberTileSize = new Vector2(numberTileDimension, numberTileDimension);
        gridLayout.cellSize = numberTileSize;
        float outlineWidth = defaultOutlineDistance * (9f / Sudoku.Size);
        outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
        
        for (int i = 1; i <= Sudoku.Size; i++) {
            NumberTile numberTile;
            if (_numberOfActiveTiles >= _numberTiles.Count) {
                numberTile = 
                    Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
                _numberTiles.Add(numberTile);
                numberTile.MyTileManager = this;
                numberTile.Number = i;
            } else {
                numberTile = _numberTiles[_numberOfActiveTiles];
            }
            numberTile.Valid = Sudoku.Valid(_index, i);
            numberTile.Cleared = cleared;
            numberTile.gameObject.SetActive(true);
            numberTile.SetSize(numberTileSize);
            _numberOfActiveTiles++;
        }
    }

    public void RemoveAllTiles() {
        while (_numberOfActiveTiles > 0) {
            _numberTiles[_numberOfActiveTiles - 1].gameObject.SetActive(false);
            _numberOfActiveTiles--;
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
        for (int number = 1; number <= Sudoku.Size; number++)
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
    
    public void SetSize(Vector2 size) => textRectTransform.sizeDelta = size;
}
