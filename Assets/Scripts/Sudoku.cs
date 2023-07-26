using System;
using System.Collections.Generic;
using Random = System.Random;

public enum Difficulty {  Impossible = 64, Improbable = 60, VeryHard = 55, Hard = 50, Medium = 45, Easy = 40 }

public class Sudoku {
    private const int Blank = 0;
    private static int _size = 9;
    private static Symmetry _symmetry = Symmetry.Vertical;
    private static int[,] Board { get; set; } = {
        { 7, 0, 2, 0, 5, 0, 6, 0, 0 },
        { 0, 0, 0, 0, 0, 3, 0, 0, 0 },
        { 1, 0, 0, 0, 0, 9, 5, 0, 0 },
        { 8, 0, 0, 0, 0, 0, 0, 9, 0 },
        { 0, 4, 3, 0, 0, 0, 7, 5, 0 },
        { 0, 9, 0, 0, 0, 0, 0, 0, 8 },
        { 0, 0, 9, 7, 0, 0, 0, 0, 5 },
        { 0, 0, 0, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 7, 0, 4, 0, 2, 0, 3 }
    };

    public Sudoku() : this(9) { }
    
    public Sudoku(int size) => _size = size;

    public static void NewPuzzle(int seed, Difficulty difficulty) {
        Random random = seed == 0 ? new Random() : new Random(seed);
        Board = NewBoard(new int[_size, _size], random);
        Board = SetDifficulty(Board, difficulty, random);
    }
    
    private static int[,] NewBoard(int[,] board, Random random) {
        List<int> rowOrder = new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        rowOrder.Shuffle(random);
        
        List<int> colOrder = new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        colOrder.Remove(rowOrder[0]);
        colOrder.Shuffle(random);

        for (int row = 0; row < rowOrder.Count; row++)
            board[row, 0] = rowOrder[row];

        for (int col = 0; col < colOrder.Count; col++)
            board[0, col + 1] = colOrder[col];

        int[,] filledBoard = Solve(board);
        
        if (filledBoard == null) // duplicate values in first box, recursively try new board
            return NewBoard(new int[board.GetLength(0), board.GetLength(1)], random);
        
        return board;
    }

    private static int[,] SetDifficulty(int[,] board, Difficulty difficulty, Random random) {
        int desiredBlanks = (int)difficulty;

        _symmetry = _symmetry.RandomEnumValue(random);
        
        board = BlankCells(board, _symmetry, desiredBlanks, random);
        return board;
    }

    private static int[,] BlankCells(int[,] board, Symmetry symmetry, int desiredBlanks, Random random, int blanks = 0, int tries = 0) {
        int[,] copy = board.Copy();
        int blanked = 0;
        int row, col; 

        do {
            row = random.Next(copy.GetLength(0));
            col = random.Next(copy.GetLength(1));
        } while (copy[row, col] == Blank);

        copy[row, col] = Blank;
        blanked++;
        
        int symmetricalRow = row;
        int symmetricalCol = col;

        switch (symmetry) {
            case Symmetry.Vertical:
                symmetricalCol = copy.GetLength(1) - 1 - col;
                break;
            case Symmetry.Horizontal:
                symmetricalRow = copy.GetLength(0) - 1 - row;
                break;
            case Symmetry.Diagonal:
                (symmetricalRow, symmetricalCol) = (symmetricalCol, symmetricalRow); // Swap.
                break;
        }

        if (copy[symmetricalRow, symmetricalCol] != Blank) {
            blanked++;
            copy[symmetricalRow, symmetricalCol] = Blank;
        }

        if (Unique(copy)) {
            blanks -= blanked;
            board = copy;
        } else
            tries++;

        if (blanks < desiredBlanks && tries < 100)
            return BlankCells(board, symmetry, desiredBlanks, random, blanks, tries);

        return board;
    }

    private static bool Unique(int[,] board) {
        int numSolvedBoards = 0;
        Unique(board.Copy(), ref numSolvedBoards);
        
        return numSolvedBoards == 1;
    }

    private static void Unique(int[,] board, ref int numSolvedBoards) {
        for (int row = 0; row < _size; row++)
            for (int col = 0; col < _size; col++)
                if (board[row, col] == 0) {
                    for (int num = 1; num <= _size; num++) {
                        if (Valid(board, num, row, col)) {
                            board[row, col] = num;      // Try num.
                            Unique(board, ref numSolvedBoards);// Recursive call.
                            board[row, col] = 0;        // Reset num before backtracking.
                            if (numSolvedBoards > 1)    // Early out. Non-unique. Optimisation.
                                return;
                        }
                    }
                    return;                             // No valid numbers possible, earlier numbers incorrect.
                }

        numSolvedBoards++;                              // Update number of solved boards.
    }
    
    public static int[,] Solve(int[,] board) {
        for (int row = 0; row < _size; row++)
            for (int col = 0; col < _size; col++)
                if (board[row, col] == 0) {
                    for (int num = 1; num <= _size; num++) {
                        if (Valid(board, num, row, col)) {
                            board[row, col] = num;      // Try num.
                            if (Solve(board) != null) { // Recursive call.
                                return board;           // Success! Managed to fill entire board!
                            }
                            board[row, col] = 0;        // Reset num before backtracking.
                        }
                    }
                    return null;                        // No valid numbers possible, earlier numbers incorrect.
                }

        return board;                                   // Success! All cells filled.
    }

    private static bool Valid(int[,] board, int num, int row, int col) {
        return !NumberInRow(board, num, row) &&
               !NumberInCol(board, num, col) &&
               !NumberInBox(board, num, row, col);
    }

    private static bool NumberInRow(int[,] board, int num, int row) {
        for (int i = 0; i < _size; i++)
            if (board[row, i] == num)
                return true;

        return false;
    }
    
    private static bool NumberInCol(int[,] board, int num, int col) {
        for (int i = 0; i < _size; i++)
            if (board[i, col] == num)
                return true;

        return false;
    }

    private static bool NumberInBox(int[,] board, int num, int row, int col) {
        int boxRow = row - row % 3;
        int boxCol = col - col % 3;

        for (int i = boxRow; i < boxRow + 3; i++)
            for (int j = boxCol; j < boxCol + 3; j++)
                if (board[i, j] == num)
                    return true;

        return false;
    }

    public static void SetNumber(int index, int number) {
        int row = index / _size;
        int col = index % _size;
        Board[row, col] = number;
    }

    private static int RowFromIndex(int index) => index / _size;
    
    private static int ColFromIndex(int index) => index % _size;
    
    private static int Row(int index) => index / _size;
    
    private static int Col(int index) => index % _size;
    
    public static int NumberAt(int index) => Board[Row(index), Col(index)];

    public static int RowStartIndex(int index) => Row(index) * _size;

    public static int ColStartIndex(int index) => Col(index);

    public static int BoxStartIndex(int index) => Row(index) / 3 * _size * 3 + Col(index) / 3 * 3;

    public static bool ValidAtIndex(int index, int num) =>
        Valid(Board, num, Row(index), Col(index));

    //public static void PrintBoard(int[,] board) => 
    //    Debug.Log(String(board));

    //private static string String(int[,] board) {
    //    string s = string.Empty;
    //    for (int i = 0; i < _size; i++) {
    //        for (int j = 0; j < _size; j++)
    //            s += board[i, j] + " ";
    //        
    //        s += "\n";
    //    }
    //
    //    return s;
    //}
}

