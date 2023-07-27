using System.Collections.Generic;
using UnityEngine;

public class SudokuManager : MonoBehaviour {
    private static int _size = 9;
    private static List<CellManager> _cellManagers = new List<CellManager>();
    
    [SerializeField] private GameObject cellPrefab;

    private void Awake() => CreateNewPuzzle(_size);

    private void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        DestroyOldPuzzle();
        Sudoku.NewPuzzle(seed, _size, difficulty);
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
        int rowStart = Sudoku.RowStartIndex(index);
        int colStart = Sudoku.ColStartIndex(index);
        int boxStart = Sudoku.BoxStartIndex(index);

        for (int row = rowStart, rowEnd = rowStart + _size; row < rowEnd; row++)
            _cellManagers[row].RemoveTiles(nums);
        
        for (int col = colStart, colEnd = _size * _size + colStart; col < colEnd; col += _size)
            _cellManagers[col].RemoveTiles(nums);
        
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                _cellManagers[boxStart + r * _size + c].RemoveTiles(nums);
    }

    public static void SetNumber(int index, int number, bool removeInvalidNumbers = true) {
        Sudoku.SetNumber(index, number);
        if (removeInvalidNumbers)
            RemoveInvalidTiles(index, number);
    }
}
