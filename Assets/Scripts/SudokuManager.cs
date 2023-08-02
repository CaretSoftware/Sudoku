#if UNITY_STANDALONE_WIN
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SudokuManager : MonoBehaviour {
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    [SerializeField] private BlockColorHandler blockColorHandler;
    
    public delegate void SudokuGenerationFinished();
    public static SudokuGenerationFinished SudokuFinished;
    private static readonly List<TileManager> TileManagers = new List<TileManager>();
    private static int _numberOfActiveCells;
    private static int _numberOfTilesSet = 0;
    private Difficulty _difficulty;
    private int _size;
    private int _seed;
#if UNITY_STANDALONE_WIN
    private bool _sudokuGenerated;

    private void Awake() => SudokuFinished += SudokuGenerated;

    private void SudokuGenerated() => _sudokuGenerated = true;

    private void OnDestroy() => SudokuFinished -= SudokuGenerated;
#endif
    
    private void Start() => CreateNewPuzzle(Sudoku.Size);

#if UNITY_STANDALONE_WIN
    public async void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        blockColorHandler.InitializeBlocks(size);
        RemoveLastPuzzle();
        _size = size;
        _seed = seed; 
        _difficulty = difficulty;
        _sudokuGenerated = false;
        Thread thread = new Thread(CreateNewPuzzle);
        thread.Start();
        await WaitForSudokuGeneration();
        blockColorHandler.ShowBlocks();
        InitializeCells(_size);
        _numberOfTilesSet = Sudoku.Board.Count(num => num != 0);
    }

    private async Task WaitForSudokuGeneration() {
        while (!_sudokuGenerated) {
            await Task.Yield();
        }
    }
    
#elif UNITY_WEBGL
    public void CreateNewPuzzle(int size, Difficulty difficulty = Difficulty.Easy, int seed = 0) {
        blockColorHandler.InitializeBlocks(size);
        RemoveLastPuzzle();
        _size = size;
        _seed = seed; 
        _difficulty = difficulty;
        CreateNewPuzzle();
        blockColorHandler.ShowBlocks();
        InitializeCells(_size);
        _numberOfTilesSet = Sudoku.Board.Count(num => num != 0);
    }
#endif

    private void RemoveLastPuzzle() {
        Command.Processor.ClearUndo();
        RemoveAllCells();
    }

    private static void RemoveAllCells() {
        while (_numberOfActiveCells > 0) {
            TileManagers[_numberOfActiveCells - 1].gameObject.SetActive(false);
            TileManagers[_numberOfActiveCells - 1].RemoveAllTiles();
            _numberOfActiveCells--;
        }
    }
    
    private void CreateNewPuzzle() => Sudoku.NewPuzzle(_seed, _size, _difficulty);

    private void InitializeCells(int size) {
        Vector2 gridCellSize = new Vector2( sudokuPanelDimension / size, sudokuPanelDimension / size);
        gridLayout.cellSize = gridCellSize;
        
        while (_numberOfActiveCells < size * size) {
            TileManager tileManager;
            if (_numberOfActiveCells >= TileManagers.Count) {   // Dynamically Instantiate more tiles
                tileManager = Instantiate(cellPrefab, this.transform).GetComponent<TileManager>();
                TileManagers.Add(tileManager);
            } else {                                            // Get tileManager from "pool"
                tileManager = TileManagers[_numberOfActiveCells];
            }

            tileManager.SetSize(gridCellSize);
            tileManager.gameObject.SetActive(true);
            _numberOfActiveCells++;
        }
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
        _numberOfTilesSet += number != 0 ? 1 : -1;
        if (_numberOfTilesSet >= Sudoku.Size * Sudoku.Size) {
            if (!InvalidNumbers(Sudoku.Board) && Sudoku.Solution(Sudoku.Board))
                WinText.UpdateEndGameText?.Invoke("CORRECT");
            else
                WinText.UpdateEndGameText?.Invoke("INCORRECT\nUNDO [Z]");
        }
    }

    public static void FillNumber(int index, int number) {
        Command.FillNumber fillNumberCommand = new Command.FillNumber(index, Sudoku.Number(index), number);
        FindInvalidTiles(index, fillNumberCommand.AddTileChangeDelegate(), number);
        Command.Processor.ExecuteCommand(fillNumberCommand);
    }
}
