using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class InputManager : MonoBehaviour {
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
    private Color lightModeButtonDisableColor;
    private SudokuManager _sudokuManager;
    private Difficulty _difficulty;
    private int _seed;
    private int _size = 9;
    private float _undoInterval;
    private float _redoInterval;
    private float _rampUp;
    private bool _darkMode;

    private void Awake() {
        Command.Processor.undoEmptyDelegate += UndoButton;
        lightModeButtonDisableColor = undoButton.colors.disabledColor;
        _sudokuManager = FindObjectOfType<SudokuManager>();
    }

    private void OnDestroy() => Command.Processor.undoEmptyDelegate -= UndoButton;

    public void NewSudoku() {
        Debug.Log($"size NewSudoku {_size}");
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
        _sudokuManager.CreateNewPuzzle(_size, _difficulty, _seed);
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
        switch (sizeDropDown.value) {
            case 0:
                _size = 9;
                break;
            case 1:
                _size = 16;
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
        NumberTile.HideInvalidTile?.Invoke(hideInvalidNumbersToggle.isOn);

    public void NightMode() {
        _darkMode = !_darkMode;

        postProcessVolume.profile = _darkMode ? darkGlobalVolumeProfile : lightGlobalVolumeProfile;
        darkModeButtonText.text = _darkMode ? "LIGHT MODE" : "DARK MODE";
        ColorBlock colorBlock = undoButton.colors;
        colorBlock.disabledColor = _darkMode ? darkModeButtonDisableColor : lightModeButtonDisableColor;
        undoButton.colors = colorBlock;
    }
    
    private void UndoButton(bool empty) => undoButton.interactable = !empty;
}
