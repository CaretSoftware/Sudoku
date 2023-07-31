using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SudokuManager : MonoBehaviour {
    public delegate void SudokuGenerationFinished();
    public static SudokuGenerationFinished SudokuFinished;
    private static readonly List<TileManager> TileManagers = new List<TileManager>();
    private static int _numberOfActiveCells;
    private bool _sudokuGenerated;
    private Difficulty _difficulty;
    private int _size;
    private int _seed;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    [SerializeField] private BlockHandler blockHandler;

    private void Awake() {
        SudokuFinished += SudokuGenerated;
    }

    private void CreateCells(int size) {
        Vector2 gridCellSize = new Vector2( sudokuPanelDimension / size, sudokuPanelDimension / size);
        gridLayout.cellSize = gridCellSize;
        
        while (_numberOfActiveCells < size * size) {
            TileManager tileManager;
            if (_numberOfActiveCells >= TileManagers.Count) {
                tileManager = Instantiate(cellPrefab, this.transform).GetComponent<TileManager>();
                TileManagers.Add(tileManager);
            } else {
                tileManager = TileManagers[_numberOfActiveCells];
            }

            tileManager.SetSize(gridCellSize);
            tileManager.gameObject.SetActive(true);
            _numberOfActiveCells++;
        }
    }

    private void RemoveAllCells() {
        while (_numberOfActiveCells > 0) {
            TileManagers[_numberOfActiveCells - 1].gameObject.SetActive(false);
            TileManagers[_numberOfActiveCells - 1].RemoveAllTiles();
            _numberOfActiveCells--;
        }
    }
    
    private void Start() => CreateNewPuzzle(Sudoku.Size);
    
    public async void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        blockHandler.NewBlocks(size);
        RemoveOldPuzzle();
        //await Sudoku.NewPuzzle(seed, size, difficulty);
        _size = size;
        _seed = seed; 
        _difficulty = difficulty;
        _sudokuGenerated = false;
        Thread thread = new Thread(CreateNewPuzzleThread);
        thread.Start();
        await WaitForSudokuGeneration();
        blockHandler.ShowBlocks();
        //InstantiateCells(_threadedSize);
        CreateCells(_size);
    }
    
    private void CreateNewPuzzleThread() => Sudoku.NewPuzzle(_seed, _size, _difficulty);

    private async Task WaitForSudokuGeneration() {
        while (!_sudokuGenerated) {
            await Task.Yield();
        }
    }

    private void SudokuGenerated() => _sudokuGenerated = true;

    private void RemoveOldPuzzle() {
        Command.Processor.ClearUndo();
        RemoveAllCells();
    }

    private static void FindInvalidTiles(int index, Command.FillNumber.AddTileChange addTileChange = null, 
            params int[] nums) {
        int rowStart = Sudoku.RowStartIndex(index);
        int colStart = Sudoku.ColStartIndex(index);
        int boxStart = Sudoku.BoxStartIndex(index);

        for (int row = rowStart, rowEnd = rowStart + Sudoku.Size; row < rowEnd; row++)
            TileManagers[row].ClearTiles(addTileChange, numbers: nums);
        
        for (int col = colStart, colEnd = Sudoku.Size * Sudoku.Size + colStart; col < colEnd; col += Sudoku.Size)
            TileManagers[col].ClearTiles(addTileChange, numbers: nums);
        
        for (int r = 0; r < Sudoku.BlockWidth; r++)
            for (int c = 0; c < Sudoku.BlockWidth; c++)
                TileManagers[boxStart + r * Sudoku.Size + c].ClearTiles(addTileChange, numbers: nums);
    }

    public void Solve() {
        if (InvalidNumbers(Sudoku.Board) || !Sudoku.Solution(Sudoku.Board)) {
            const string message = "NO VALID SOLUTION";
            WarningMessagePopup.WarningMessage?.Invoke(message);
            return;
        }
        for (int index = 0; index < Sudoku.Board.Length; index++) {
            FindInvalidTiles(index, null, Sudoku.Board[index]);
            TileManagers[index].SetNumber(Sudoku.Board[index]);
            SetNumber(index, number: Sudoku.Board[index]);
        }
        Command.Processor.ClearUndo();
    }

    private static bool InvalidNumbers(int[] board) {
        for (int index = 0; index < board.Length; index++) {
            int num = board[index];
            if (num == Sudoku.Blank) continue;
            board[index] = Sudoku.Blank;
            bool invalid = !Sudoku.Valid(board, num, index);
            board[index] = num;
            if (invalid)
                return true;
        }
        return false;
    }

    public static void SetNumber(int index, int number) {
        Sudoku.SetNumber(index, number);
        TileManagers[index].SetNumber(number);
    }

    public static void FillNumber(int index, int number) {
        Command.FillNumber fillNumberCommand = new Command.FillNumber(index, Sudoku.Number(index), number);
        FindInvalidTiles(index, fillNumberCommand.AddTileChangeDelegate(), number);
        Command.Processor.ExecuteCommand(fillNumberCommand);
    }

    private void OnDestroy() => SudokuFinished -= SudokuGenerated;
}
