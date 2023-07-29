using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum Difficulty {  Hard = 64, Medium = 45, Easy = 40 }

public class Sudoku {
    public const int Blank = 0;
    private static int _size {
        get => _sizeBackingField;
        set {
            _sizeBackingField = value;
            _blockWidth = Mathf.RoundToInt(Mathf.Sqrt(value));
            Debug.Log($"blockWidth {_blockWidth}");
        }
    }
    private static int _sizeBackingField = 9;
    private static int _blockWidth = 3;
    private static Symmetry _symmetry = Symmetry.Vertical;
    public static int[] Board { get; private set; } = {
        7, 0, 2, 0, 5, 0, 6, 0, 0,
        0, 0, 0, 0, 0, 3, 0, 0, 0,
        1, 0, 0, 0, 0, 9, 5, 0, 0,
        8, 0, 0, 0, 0, 0, 0, 9, 0,
        0, 4, 3, 0, 0, 0, 7, 5, 0,
        0, 9, 0, 0, 0, 0, 0, 0, 8,
        0, 0, 9, 7, 0, 0, 0, 0, 5,
        0, 0, 0, 2, 0, 0, 0, 0, 0,
        0, 0, 7, 0, 4, 0, 2, 0, 3,
    };

    public static void NewPuzzle(int seed, int size, Difficulty difficulty) {
        _size = size;
        Random random = seed == 0 ? new Random() : new Random(seed);
        Board = NewBoard(new int[size * size], size, random);
        //Board = SetDifficulty(Board, size, difficulty, random);
    }
    
    private static int[] NewBoard(int[] board, int size, Random random) {
        List<int> rowNumbers = new List<int>(size);
        for (int num = 1; num <= size; num++)
            rowNumbers.Add(num);
        
        rowNumbers.Shuffle(random);
        
        List<int> colOrder = new List<int>();
        for (int num = 1; num <= size; num++)
            if (num != rowNumbers[0])
                colOrder.Add(num);
        
        colOrder.Shuffle(random);

        for (int row = 0, num = 0; row < size * size; row += size)
            board[row] = rowNumbers[num++];

        for (int col = 0; col < size - 1; col++)
            board[col + 1] = colOrder[col];
        debug = 0;
        int[] filledBoard = Solve(board);
        PrintBoard(filledBoard);
        //if (filledBoard == null) // duplicate values in first block, recursively try new board
        //    return NewBoard(new int[size * size], size, random);
        
        return board;
    }

    private static void PrintBoard(int[] board) {
        string s = String.Format("==={0, -3}x{0, 3}===\n", _size);
        for (int i = 0; i < board.Length; i++) {
            if ((i) % _size == 0)
                s += "\n";
            s += $"{board[i]} ";
        }
        Debug.Log(s);
    }

    private static int[] SetDifficulty(int[] board, int size, Difficulty difficulty, Random random) {
        int desiredBlanks = (int)difficulty;

        _symmetry = _symmetry.RandomEnumValue<Symmetry>(random);
        
        board = BlankCells(board, size, _symmetry, desiredBlanks, random);
        return board;
    }

    private static int[] BlankCells(int[] board, int size, Symmetry symmetry, int desiredBlanks, Random random, int blanks = 0, int tries = 0) {
        int[] copy = (int[])board.Clone();
        int length = copy.Length;
        int blanked = 0;
        int index;

        do {
            index = random.Next(length);
        } while (copy[index] == Blank);

        copy[index] = Blank;
        blanked++;
        
        switch (symmetry) {
            case Symmetry.Vertical:
                index = RowStartIndex((size - 1 - Row(index)) * size) + Col(index);
                break;
            case Symmetry.Horizontal:
                index = RowStartIndex(index) + size - 1 - Col(index);
                break;
            case Symmetry.Diagonal:
                index = length - 1 - index;
                break;
        }

        if (copy[index] != Blank) {
            blanked++;
            copy[index] = Blank;
        }

        if (Unique(copy)) {
            blanks += blanked;
            board = copy;
        } else
            tries++;

        if (blanks < desiredBlanks && tries < 100)
            return BlankCells(board, size, symmetry, desiredBlanks, random, blanks, tries);

        return board;
    }

    private static bool Unique(int[] board) {
        int numSolvedBoards = 0;
        Unique((int[])board.Clone(), ref numSolvedBoards);
        
        return numSolvedBoards == 1;
    }

    private static void Unique(int[] board, ref int numSolvedBoards) {
        int length = board.Length;
        for (int index = 0; index < length; index++) {
            if (board[index] == Blank) {
                for (int num = 1; num <= _size; num++) {
                    if (Valid(board, num, index)) {
                        board[index] = num;             // Try num.
                        Unique(board, ref numSolvedBoards);// Recursive call.
                        board[index] = 0;               // Reset num before backtracking.
                        if (numSolvedBoards > 1)        // Early out. Non-unique. Optimisation.
                            return;
                    }
                }
                return;                                 // No valid numbers possible, earlier numbers incorrect.
            }
        }

        numSolvedBoards++;                              // Update number of solved boards.
    }

    public static bool Solution(int[] board) {
        board = (int[])board.Clone();
        if (Solve(board) == null)
            return false;
        Board = board;
        return true;
    }

    private static int debug;
    private static int[] Solve(int[] board) {
        int length = board.Length;
        for (int index = 0; index < length; index++) {
            if (board[index] == Blank) {
                for (int num = 1; num <= _size; num++) {
                    if (Valid(board, num, index)) {
                        debug++;
                        if (debug > 100000) {
                            WarningMessage.warningMessage?.Invoke("10.000 ITERATIONS\nNO SUDOKU");
                            return null;
                        }
                        board[index] = num;             // Try num.
                        if (Solve(board) != null) {     // Recursive call.
                            Board = board;
                            return board;               // Success! Managed to fill entire board!
                        }
                        board[index] = 0;               // Reset num before backtracking.
                    }
                }
                return null;                            // No valid numbers possible, earlier numbers incorrect.
            }
        }

        return board;                                   // Success! All cells filled.
    }

    private static bool Valid(int[] board, int num, int index) {
        return board[index] == Blank &&
               !NumberInRow(board, num, index) &&
               !NumberInCol(board, num, index) &&
               !NumberInBox(board, num, index);
    }

    private static bool NumberInRow(int[] board, int num, int index) {
        int rowStartIndex = RowStartIndex(index);
        for (int i = 0; i < _size; i++)
            if (board[rowStartIndex + i] == num)
                return true;
        
        return false;
    }

    private static bool NumberInCol(int[] board, int num, int index) {
        int colStartIndex = ColStartIndex(index);
        for (int i = 0; i < _size * _size; i += _size)
            if (board[colStartIndex + i] == num)
                return true;

        return false;
    }

    private static bool NumberInBox(int[] board, int num, int index) {
        int boxStart = BoxStartIndex(index);
        for (int r = 0; r < _blockWidth; r++)
            for (int c = 0; c < _blockWidth; c++)
                if (board[boxStart + r * _size + c] == num)
                    return true;
        
        return false;
    }

    private static int Row(int index) => index / _size;
    
    private static int Col(int index) => index % _size;
    
    public static int RowStartIndex(int index) => Row(index) * _size;
    
    public static int ColStartIndex(int index) => Col(index);

    public static int BoxStartIndex(int index) => 
        Row(index) / _blockWidth * _size * _blockWidth + Col(index) / _blockWidth * _blockWidth;

    public static bool Valid(int index, int num) => Valid(Board, num, index);

    public static int Number(int index) => Board[index];

    public static void SetNumber(int index, int number) => Board[index] = number;
}

