using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class SudokuManager : MonoBehaviour {
    private static int _size = 9;
    private static List<CellManager> _cellManagers = new List<CellManager>();
    [SerializeField] private GameObject cellPrefab;

    //private void Awake() => CreateNewPuzzle(_size);

    public void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        _size = size;
        DestroyOldPuzzle();
        Debug.Log($"DestroyOldPuzzle");
        Sudoku.NewPuzzle(seed, size, difficulty);
        Debug.Log($"NewPuzzle");
        InstantiateCells(size);
        Debug.Log($"InstantiateCells {size}");
    }
    
    private void DestroyOldPuzzle() {
        Command.Processor.ClearUndo();
        for (int i = 0; i < _cellManagers.Count; i++) 
            Destroy(_cellManagers[i].gameObject);
        _cellManagers.Clear();
    }
    
    private void InstantiateCells(int size) {
        CellManager.ResetCells(size);
        for (int index = 0; index < size * size; index++)
            _cellManagers.Add(Instantiate(cellPrefab, this.transform).GetComponent<CellManager>());
    }

    private static void FindInvalidTiles(int index, 
            Command.FillNumber.AddTileChange addTileChange = null, params int[] nums) {
        int rowStart = Sudoku.RowStartIndex(index);
        int colStart = Sudoku.ColStartIndex(index);
        int boxStart = Sudoku.BoxStartIndex(index);

        for (int row = rowStart, rowEnd = rowStart + _size; row < rowEnd; row++)
            _cellManagers[row].ClearTiles(addTileChange, numbers: nums);
        
        for (int col = colStart, colEnd = _size * _size + colStart; col < colEnd; col += _size)
            _cellManagers[col].ClearTiles(addTileChange, numbers: nums);
        
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                _cellManagers[boxStart + r * _size + c].ClearTiles(addTileChange, numbers: nums);
    }

    public void Solve() {
        if (!Sudoku.Solution(Sudoku.Board)) {
            const string message = "NO VALID SOLUTION";
            WarningMessage.warningMessage?.Invoke(message);
            return;
        }
        for (int index = 0; index < Sudoku.Board.Length; index++) {
            FindInvalidTiles(index, null, Sudoku.Board[index]);
            _cellManagers[index].SetNumber(Sudoku.Board[index]);
            SetNumber(index, number: Sudoku.Board[index]);
        }
        Command.Processor.ClearUndo();
    }

    public static void SetNumber(int index, int number) {
        Sudoku.SetNumber(index, number);
        _cellManagers[index].SetNumber(number);
    }

    public static void FillNumber(int index, int number) {
        Command.FillNumber fillNumberCommand = new Command.FillNumber(index, Sudoku.Number(index), number);
        FindInvalidTiles(index, fillNumberCommand.AddTileChangeDelegate(), number);
        Command.Processor.ExecuteCommand(fillNumberCommand);
    }
}
