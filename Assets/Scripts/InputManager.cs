using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InputManager : MonoBehaviour {

    [SerializeField] private TMP_Dropdown sudokuDropDown;
    [SerializeField] private TMP_Dropdown sizeDropDown;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Toggle hideInvalidNumbersToggle;
    [SerializeField] private TextMeshProUGUI darkModeText;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private VolumeProfile darkGlobalVolumeProfile;
    [SerializeField] private VolumeProfile lightGlobalVolumeProfile;
    private ColorCurves _colorCurves;
    
    private SudokuManager _sudokuManager;
    private int _seed;
    private int _size = 9;
    private Difficulty _difficulty;
    private bool _darkMode;

    private void Awake() => _sudokuManager = FindObjectOfType<SudokuManager>();

    private void Start() => postProcessVolume.profile.TryGet(out _colorCurves);

    public void NewSudoku() {
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

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Backspace) ||
            ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) 
             && Input.GetKeyDown(KeyCode.Z))) {
            Debug.Log("pressed undo");
            Command.Processor.Undo();
        }

        if (Input.GetKeyDown(KeyCode.N)) {
            Debug.Log("pressed redo");
            Command.Processor.Redo();
        }
    }

    public void Solve() {
        _sudokuManager.Solve();
        Debug.Log($"solve");
    }

    public void Undo() { 
        Command.Processor.Undo();
    }

    public void Size() {
        Debug.Log($"size: {sizeDropDown.value}");
        switch (sizeDropDown.value) {
            case 0:
                _size = 9;
                break;
            case 1:
                _size = 15;
                break;
        }
    }
 
    public void Seed() {
        string text =
            seedInputField.text;
        int seed = 0;
        for (int c = text.Length - 1, place = 1; c >= 0; c--, place *= 10) {
            seed += (Char.IsNumber(text[c])
                    ? Mathf.RoundToInt((float)Char.GetNumericValue(text[c]))
                    : (text[c] - 6) % 10) * place;
        }

        _seed = seed;
        seedInputField.text = seed == 0 ? string.Empty : seed.ToString();
    }

    public void ShowValidNumbers() =>
        NumberTile.ShowValidDelegate?.Invoke(hideInvalidNumbersToggle.isOn);

    public void NightMode() {
        _darkMode = !_darkMode;
        postProcessVolume.profile = _darkMode ? darkGlobalVolumeProfile : lightGlobalVolumeProfile;
        darkModeText.text = _darkMode ? "LIGHT MODE" : "DARK MODE";
    }
}
