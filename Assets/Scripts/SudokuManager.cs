using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokuManager : MonoBehaviour {
    private static int _size = 9;
    private static List<TileManager> _tileManagers = new List<TileManager>();
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform sudokuRectTransform;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    
    private void Awake() => CreateNewPuzzle(_size);

    public void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        _size = size;
        DestroyOldPuzzle();
        Sudoku.NewPuzzle(seed, size, difficulty);
        InstantiateCells(size);
    }
    
    private void DestroyOldPuzzle() {
        Command.Processor.ClearUndo();
        for (int i = 0; i < _tileManagers.Count; i++) 
            Destroy(_tileManagers[i].gameObject);
        _tileManagers.Clear();
    }
    
    private void InstantiateCells(int size) {
        TileManager.ResetCells(size);
        Vector2 cellSize = new Vector2( sudokuPanelDimension / size, sudokuPanelDimension / size);
        gridLayout.cellSize = cellSize;

        for (int index = 0; index < size * size; index++) {
            TileManager tileManager = Instantiate(cellPrefab, this.transform).GetComponent<TileManager>();
            _tileManagers.Add(tileManager);
            tileManager.SetSize(cellSize);
        }
    }

    private static void FindInvalidTiles(int index, 
            Command.FillNumber.AddTileChange addTileChange = null, params int[] nums) {
        int rowStart = Sudoku.RowStartIndex(index);
        int colStart = Sudoku.ColStartIndex(index);
        int boxStart = Sudoku.BoxStartIndex(index);

        for (int row = rowStart, rowEnd = rowStart + _size; row < rowEnd; row++)
            _tileManagers[row].ClearTiles(addTileChange, numbers: nums);
        
        for (int col = colStart, colEnd = _size * _size + colStart; col < colEnd; col += _size)
            _tileManagers[col].ClearTiles(addTileChange, numbers: nums);
        
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                _tileManagers[boxStart + r * _size + c].ClearTiles(addTileChange, numbers: nums);
    }

    public void Solve() {
        
        if (InvalidNumbers(Sudoku.Board) || !Sudoku.Solution(Sudoku.Board)) {
            const string message = "NO VALID SOLUTION";
            WarningMessage.warningMessage?.Invoke(message);
            return;
        }
        for (int index = 0; index < Sudoku.Board.Length; index++) {
            FindInvalidTiles(index, null, Sudoku.Board[index]);
            _tileManagers[index].SetNumber(Sudoku.Board[index]);
            SetNumber(index, number: Sudoku.Board[index]);
        }
        Command.Processor.ClearUndo();
    }

    private static bool InvalidNumbers(int[] board) {
        for (int index = 0; index < board.Length; index++) {
            int num = board[index];
            if (num == Sudoku.Blank) continue;
            board[index] = Sudoku.Blank;
            if (!Sudoku.Valid(board, num, index)) {
                return true;
            }
            board[index] = num;
        }
        return false;
    }

    public static void SetNumber(int index, int number) {
        Sudoku.SetNumber(index, number);
        _tileManagers[index].SetNumber(number);
    }

    public static void FillNumber(int index, int number) {
        Command.FillNumber fillNumberCommand = new Command.FillNumber(index, Sudoku.Number(index), number);
        FindInvalidTiles(index, fillNumberCommand.AddTileChangeDelegate(), number);
        Command.Processor.ExecuteCommand(fillNumberCommand);
    }
}
