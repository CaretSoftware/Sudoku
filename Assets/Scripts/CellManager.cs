using System;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CellManager : MonoBehaviour {
    public delegate void ShowValid();
    public static ShowValid ShowValidDelegate;

    private static int _size;
    private readonly NumberTile[] _numberTiles = new NumberTile[_size];
    private static int _staticIndex;
    private int _index;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject numberTilePrefab;
    [SerializeField] private GameObject blankTilePrefab;
    [SerializeField] private Transform numberTileParent;

    private void Awake() {
        _index = _staticIndex++;
        ShowValidDelegate += ShowValidNumbers;
    }

    private void OnDestroy() => ShowValidDelegate -= ShowValidNumbers;

    private void ShowValidNumbers() {
        
    }

    private void Start() => FillCell();

    private void FillCell() {
        int number = Sudoku.Number(_index);

        if (number == 0)
            SpawnTiles();
        else
            text.text = number.ToString();
    }

    private void SpawnTiles() {
        for (int i = 1; i <= _size; i++) {
            if (Sudoku.Valid(_index, i)) {
                NumberTile numberTile = 
                    Instantiate(numberTilePrefab, numberTileParent.transform).GetComponent<NumberTile>();
                numberTile.MyCellManager = this;
                numberTile.Number = i;
                _numberTiles[i - 1] = numberTile;
            } else {
                Instantiate(blankTilePrefab, numberTileParent.transform);
                _numberTiles[i - 1] = null;
            }
        }
    }

    public void ClickedTile(int number) {
        SetNumber(number);
        SudokuManager.SetNumber(_index, number);
        DestroyAllTiles();
    }

    public void SetNumber(int number) =>
        text.text = number.ToString();

    public void RemoveTiles(params int[] numbers) {
        for (int i = 0; i < numbers.Length; i++)
            DestroyTile(numbers[i]);
    }

    private void DestroyAllTiles() {
        for (int number = 1; number <= _size; number++)
            DestroyTile(number);
    }
    
    private void DestroyTile(int number) {
        NumberTile tile = _numberTiles[number - 1];
        if (tile != null)
            tile.Clear(true);
            //Destroy(tile);
    }

    public static void ResetCells(int size) {
        _staticIndex = 0;
        _size = size;
    }
}
