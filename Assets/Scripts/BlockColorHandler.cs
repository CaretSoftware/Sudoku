using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockColorHandler : MonoBehaviour {
    private readonly List<Image> _instantiatedFilledBlocks = new List<Image>();
    
    [SerializeField] private GameObject filledBlockPrefab;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    [SerializeField] private Color lightColor;
    [SerializeField] private Color darkColor;
    
    private int _numberOfBlocks;
    private int _lastSize;
    private int _sideDimension;
    private bool _darkMode;
    
    private void Awake() {
        InputManager.DarkModeDelegate += ColorMode;
        InitializeBlocks(Sudoku.Size);
    }

    public void InitializeBlocks(int size) {
        if (size == _lastSize) return;
        SetAllBlocksInactive(); 
        _sideDimension = Mathf.RoundToInt(Mathf.Sqrt(size));
        float blockDimension = sudokuPanelDimension / _sideDimension;
        gridLayoutGroup.constraintCount = _sideDimension;
        gridLayoutGroup.cellSize = new Vector2(blockDimension, blockDimension);

        int index = 0;
        for (int x = 0; x < _sideDimension; x++) {
            for (int y = 0; y < _sideDimension; y++) {
                Image image;
                if (_numberOfBlocks >= _instantiatedFilledBlocks.Count) {
                    image = Instantiate(filledBlockPrefab, this.transform).GetComponent<Image>();
                    _instantiatedFilledBlocks.Add(image);
                } else {
                    image = _instantiatedFilledBlocks[index++];
                }
                _numberOfBlocks++;
                image.gameObject.SetActive(true);
            }
        }

        ColorMode(_darkMode);
        gameObject.SetActive(false);
    }

    private void ColorMode(bool dark) {
        _darkMode = dark;
        Color color = dark ? darkColor: lightColor;
        int index = 0;
        for (int x = 0; x < _sideDimension; x++) {
            for (int y = 0; y < _sideDimension; y++) {
                if (FilledSquare(x, y))
                    _instantiatedFilledBlocks[index++].color = color;
                else
                    _instantiatedFilledBlocks[index++].color = Color.clear;
            }
        }
    }

    private void SetAllBlocksInactive() {
        for (int i = 0; i < _instantiatedFilledBlocks.Count; i++) 
            _instantiatedFilledBlocks[i].gameObject.SetActive(false);
        
        _numberOfBlocks = 0;
    }

    private bool FilledSquare(int x, int y) => (x + y) % 2 == 0;

    public void ShowBlocks() => gameObject.SetActive(true);

    private void OnDestroy() => InputManager.DarkModeDelegate -= ColorMode;
}
