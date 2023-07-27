using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputManager : MonoBehaviour {

    [SerializeField]
    private TMP_Dropdown sudokuDropDown;

    [SerializeField]
    private TMP_InputField seedInputField;

    private SudokuManager _sudokuManager;
    private int _seed;
    private int _size = 9;
    private Difficulty _difficulty;
    private bool _nightMode;
    private bool _showValidNumbers;

    private void Awake() {
        _sudokuManager = FindObjectOfType<SudokuManager>();
    }

    public void NewSudoku() {
        Debug.Log($"difficulty: {sudokuDropDown.value}");
        switch (sudokuDropDown.value) {
            case 0:
                _difficulty = Difficulty.Easy;
                break;
            case 1:
                _difficulty = Difficulty.Medium;
                break;
            case 2:
                _difficulty = Difficulty.Hard;
                break;
            case 3:
                _difficulty = Difficulty.VeryHard;
                break;
            case 4:
                _difficulty = Difficulty.Improbable;
                break;
            case 5:
                _difficulty = Difficulty.Impossible;
                break;
        }
        _sudokuManager.CreateNewPuzzle(_size, _difficulty, _seed);
    }

    public void Solve() {
        _sudokuManager.Solve();
        Debug.Log($"solve");
    }

    public void Undo() {
        Debug.Log($"undo"); 
    }

    public void Size(int size) {
        Debug.Log($"size: {size}");
        _size = size;
    }
 
    public void Seed() {
        if (debug > 1000) {
            Debug.LogError("eh");
            return;
        }
            
        string text =
            seedInputField.text;
        int seed = 0;
        for (int c = text.Length - 1, place = 1; c >= 0; c--, place *= 10) {
            seed += (Char.IsNumber(text[c])
                    ? Mathf.RoundToInt((float)Char.GetNumericValue(text[c]))
                    : text[c] % 10 - 6) * place;
        }

        _seed = seed;
        seedInputField.text = seed == 0 ? string.Empty : seed.ToString();
    }

    public void ShowValidNumbers() {
        _showValidNumbers = !_showValidNumbers;
    }

    public void NightMode() {
        _nightMode = !_nightMode;
    }

    private int debug = 0;
    
}
