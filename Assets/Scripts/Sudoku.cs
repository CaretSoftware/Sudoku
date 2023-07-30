using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum Difficulty {  Hard, Medium, Easy }

public class Sudoku {
    private static readonly int[,] SizeDifficultyDesiredBlanks = new int[,] {
        {12,  12,  12 }, //  4x4
        {64,  45,  40 }, //  9x9
        {166, 144, 122}, // 16x16
    };
    // Good seeds:
    private static readonly int[] GoodSeeds = new int[] { 3, 5, 6, 8, }; // 10 tried
    private static readonly Dictionary<int, int> SizeIndex = new Dictionary<int, int> { {4,  0}, {9,  1}, {16, 2} };
    public const int Blank = 0;
    public static int Size {
        get => _sizeBackingField;
        private set {
            _sizeBackingField = value;
            BlockWidth = Mathf.RoundToInt(Mathf.Sqrt(value));
        }
    }
    private static int _sizeBackingField = 9;
    public static int BlockWidth { get; private set; } = 3;
    private static Symmetry _symmetry = Symmetry.Vertical;
    private static bool _randomSeed;
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
        Size = size;
        _randomSeed = seed == 0;
        Random random = _randomSeed ? new Random() : new Random(seed);
        if (size == 16) random = new Random( GoodSeeds[random.Next(GoodSeeds.Length)] );
        Board = NewBoard(new int[size * size], size, random);
        Random tempRand = new Random();
        while (Size == 16 && tempRand.Next() % 5 == 0) { random.Next(); }
        Board = SetDifficulty(Board, size, difficulty, random);
    }
    
    private static int[] NewBoard(int[] board, int size, Random random) {
        int blockDimension = Mathf.RoundToInt(Mathf.Sqrt(size));
        List<int> rowNumbers = new List<int>(size);
        for (int num = 1; num <= size; num++)
            rowNumbers.Add(num);
        
        rowNumbers.Shuffle(random);
        
        List<int> colOrder = new List<int>();
        for (int num = 1; num <= size; num++)
            colOrder.Add(num);
        
        colOrder.Shuffle(random);

        for (int row = blockDimension * size, num = blockDimension; row < size * size; row += size)
            board[row] = rowNumbers[num++];

        for (int col = blockDimension; col < size; col++)
            board[col] = colOrder[col];

        int[] filledBoard = Solve(board);
        if (filledBoard == null) // recursively try new board when others didnt solve
            return NewBoard(new int[size * size], size, random);

        return board;
    }

    private static void PrintBoard(int[] board) {
        string s = String.Format("==={0, -3}x{0, 3}===\n", Size);
        for (int i = 0; i < board.Length; i++) {
            if (i % Size == 0)
                s += "\n";
            s += $"{board[i]} ";
        }
        Debug.Log(s);
    }

    private static int[] SetDifficulty(int[] board, int size, Difficulty difficulty, Random random) {
        int desiredBlanks = SizeDifficultyDesiredBlanks[SizeIndex[size], (int)difficulty];
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
                for (int num = 1; num <= Size; num++) {
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

    private static int[] Solve(int[] board) {
        int length = board.Length;
        for (int index = 0; index < length; index++) {
            if (board[index] == Blank) {
                for (int num = 1; num <= Size; num++) {
                    if (Valid(board, num, index)) {
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

    public static bool Valid(int[] board, int num, int index) {
        return board[index] == Blank &&
               !NumberInRow(board, num, index) &&
               !NumberInCol(board, num, index) &&
               !NumberInBox(board, num, index);
    }

    private static bool NumberInRow(int[] board, int num, int index) {
        int rowStartIndex = RowStartIndex(index);
        for (int i = 0; i < Size; i++)
            if (board[rowStartIndex + i] == num)
                return true;
        
        return false;
    }

    private static bool NumberInCol(int[] board, int num, int index) {
        int colStartIndex = ColStartIndex(index);
        for (int i = 0; i < Size * Size; i += Size)
            if (board[colStartIndex + i] == num)
                return true;

        return false;
    }

    private static bool NumberInBox(int[] board, int num, int index) {
        int boxStart = BoxStartIndex(index);
        for (int r = 0; r < BlockWidth; r++)
            for (int c = 0; c < BlockWidth; c++)
                if (board[boxStart + r * Size + c] == num)
                    return true;
        
        return false;
    }

    private static int Row(int index) => index / Size;
    
    private static int Col(int index) => index % Size;
    
    public static int RowStartIndex(int index) => Row(index) * Size;
    
    public static int ColStartIndex(int index) => Col(index);

    public static int BoxStartIndex(int index) => 
        Row(index) / BlockWidth * Size * BlockWidth + Col(index) / BlockWidth * BlockWidth;

    public static bool Valid(int index, int num) => Valid(Board, num, index);

    public static int Number(int index) => Board[index];

    public static void SetNumber(int index, int number) => Board[index] = number;
}

