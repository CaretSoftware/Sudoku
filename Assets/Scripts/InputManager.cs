using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class InputManager : MonoBehaviour {
    public delegate void DarkMode(bool dark);
    public static DarkMode DarkModeDelegate;

    private static readonly int[] BoardSizeIndex = new int[] { 4, 9, 16 };
    
    [SerializeField] private TMP_Dropdown sudokuDropDown;
    [SerializeField] private TMP_Dropdown sizeDropDown;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Button undoButton;
    [SerializeField] private Toggle hideInvalidNumbersToggle;
    [SerializeField] private TextMeshProUGUI darkModeButtonText;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private VolumeProfile darkGlobalVolumeProfile;
    [SerializeField] private VolumeProfile lightGlobalVolumeProfile;
    [SerializeField] private float repeatInputInterval = .15f;
    [SerializeField] private float repeatInputIntervalInitial = .45f;
    [SerializeField] private Color darkModeButtonDisableColor;
    
    private SudokuManager _sudokuManager;
    private Color _lightModeButtonDisableColor;
    private Difficulty _difficulty;
    private int _seed;
    private int _selectedSize = Sudoku.Size;
    private float _undoInterval;
    private float _redoInterval;
    private float _rampUp;
    private bool _darkMode;

    private void Awake() {
        Command.Processor.UndoEmptyDelegate += UndoButton;
        _lightModeButtonDisableColor = undoButton.colors.disabledColor;
        _sudokuManager = FindObjectOfType<SudokuManager>();
    }

    private void OnDestroy() => Command.Processor.UndoEmptyDelegate -= UndoButton;

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
        }

        if (_selectedSize == 16 && _difficulty != Difficulty.Easy) {
            WarningMessagePopup.WarningMessage?.Invoke("TIME LIMIT RESTRICTION\n(ONLY EASY DIFFICULTY ALLOWED FOR 16x16)");
            return;
        }
        _sudokuManager.CreateNewPuzzle(_selectedSize, _difficulty, _seed);
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Z) && Time.time > _undoInterval) {
            _undoInterval = Time.time + Mathf.Lerp(
                repeatInputIntervalInitial,
                repeatInputInterval, _rampUp += .2f);
            Command.Processor.Undo();
        }

        if (Input.GetKey(KeyCode.Y) && Time.time > _redoInterval) {
            _redoInterval = Time.time + Mathf.Lerp(
                repeatInputIntervalInitial,
                repeatInputInterval, _rampUp += .2f);
            Command.Processor.Redo();
        }
        
        if (Input.GetKeyUp(KeyCode.Z)) _undoInterval = _rampUp = 0f;
        if (Input.GetKeyUp(KeyCode.Y)) _redoInterval = _rampUp = 0f;
    }

    public void Solve() => _sudokuManager.Solve();

    public void Undo() => Command.Processor.Undo();

    public void Size() {
        _selectedSize = BoardSizeIndex[sizeDropDown.value];
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
        NumberTile.HideInvalidTile?.Invoke(hideInvalidNumbersToggle.isOn);

    public void NightMode() {
        _darkMode = !_darkMode;
        DarkModeDelegate?.Invoke(_darkMode);

        postProcessVolume.profile = _darkMode ? darkGlobalVolumeProfile : lightGlobalVolumeProfile;
        darkModeButtonText.text = _darkMode ? "LIGHT MODE" : "DARK MODE";
        ColorBlock colorBlock = undoButton.colors;
        colorBlock.disabledColor = _darkMode ? darkModeButtonDisableColor : _lightModeButtonDisableColor;
        undoButton.colors = colorBlock;
    }
    
    private void UndoButton(bool empty) => undoButton.interactable = !empty;
}
