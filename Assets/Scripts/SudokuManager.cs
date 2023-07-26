using System.Collections.Generic;
using UnityEngine;

public class SudokuManager : MonoBehaviour {
    private static int _size = 9;
    private static List<CellManager> _cellManagers = new List<CellManager>();
    private Sudoku _sudoku;
    
    [SerializeField] private GameObject cellPrefab;

    private void Awake() => CreateNewPuzzle(_size);

    private void CreateNewPuzzle(int size) {
        _sudoku = new Sudoku(size);
        DestroyOldPuzzle();
        Sudoku.NewPuzzle(seed: 0, Difficulty.Easy);
        InstantiateCells(_size);
    }

    private void DestroyOldPuzzle() {
        for (int i = 0; i < _cellManagers.Count; i++) 
            Destroy(_cellManagers[i].gameObject);
        _cellManagers.Clear();
    }

    private void InstantiateCells(int size) {
        CellManager.Size = size;
        for (int index = 0; index < size * size; index++)
            _cellManagers.Add(Instantiate(cellPrefab, this.transform).GetComponent<CellManager>());
    }

    private static void RemoveInvalidTiles(int index, params int[] nums) {
        int row = Sudoku.RowFromIndex(index);
        int col = Sudoku.ColFromIndex(index);
        
        for (int c = 0; c < _size; c++) // remove tiles in row
            _cellManagers[IndexFromCoords(row, c)].RemoveTiles(nums);
        
        for (int r = 0; r < _size; r++) // remove tiles in column
            _cellManagers[IndexFromCoords(r, col)].RemoveTiles(nums);
        
        (int, int) boxRowColStart = Sudoku.BoxRowColStart(index); // remove tiles in box
        for (int boxRow = boxRowColStart.Item1; boxRow < boxRowColStart.Item1+ 3; boxRow++)
            for (int boxCol = boxRowColStart.Item2; boxCol < boxRowColStart.Item2 + 3; boxCol++) {
                CellManager cell = _cellManagers[IndexFromCoords(boxRow, boxCol)];
                cell.RemoveTiles(nums);
            }
    }

    public static void SetNumber(int index, int number, bool removeInvalidNumbers = true) {
        Sudoku.SetNumber(index, number);
        if (removeInvalidNumbers)
            RemoveInvalidTiles(index, number);
    }
    
    private static int IndexFromCoords(int row, int col) => row * _size + col;
}
