using System;
using UnityEngine;
using TMPro;

public class CellManager : MonoBehaviour {
    public static int Size { get; set; }
    private readonly NumberTile[] _numberTiles = new NumberTile[Size];
    private static int _staticIndex;
    private int _index;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private GameObject blankTilePrefab;
    [SerializeField] private Transform numberTileParent;

    private void Awake() => _index = _staticIndex++;

    private void Start() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(_index);

        if (number == 0)
            SpawnTiles();
        else
            text.text = number.ToString();
    }

    private void SpawnTiles() {
        for (int i = 1; i <= Size; i++) {
            if (Sudoku.Valid(_index, i)) {
                NumberTile numberTile = 
                    Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
                numberTile.cellManager = this;
                numberTile.Number = i;
                _numberTiles[i - 1] = numberTile;
            } else {
                Instantiate(blankTilePrefab, numberTileParent.transform);
                _numberTiles[i - 1] = null;
            }
        }
    }

    public void ClickedTile(int number) {
        text.text = number.ToString();
        SudokuManager.SetNumber(_index, number);
        DestroyAllTiles();
    }

    public void RemoveTiles(params int[] numbers) {
        for (int i = 0; i < numbers.Length; i++)
            DestroyTile(numbers[i]);
    }

    private void DestroyAllTiles() {
        for (int number = 1; number <= Size; number++)
            DestroyTile(number);
    }
    
    private void DestroyTile(int number) {
        NumberTile tile = _numberTiles[number - 1];
        if (tile != null)
            Destroy(tile);
    }

    private void OnDestroy() => _staticIndex = 0;
}
