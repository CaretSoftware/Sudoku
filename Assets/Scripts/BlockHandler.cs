using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockHandler : MonoBehaviour {
    [SerializeField] private GameObject blankBlockPrefab;
    [SerializeField] private GameObject filledBlockPrefab;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    
    private readonly List<GameObject> _instantiatedBlocks = new List<GameObject>();
    private readonly List<Image> _instantiatedFilledBlocks = new List<Image>();
    private Color _lightColor;
    private Color _darkColor;
    private int _lastSize;
    
    private void Awake() {
        NewBlocks(Sudoku.Size);
        InputManager.DarkModeDelegate += DarkModeColor;
    }

    private void OnDestroy() => InputManager.DarkModeDelegate -= DarkModeColor;

    public void NewBlocks(int size) {
        if (size == _lastSize) return;
        DestroyOldBlocks();
        int length = Mathf.RoundToInt(Mathf.Sqrt(size));
        float blockDimension = sudokuPanelDimension / length;
        gridLayoutGroup.constraintCount = length;
        gridLayoutGroup.cellSize = new Vector2(blockDimension, blockDimension);

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < length; y++) {
                GameObject go;
                go = Instantiate(FilledSquare(x, y) ? filledBlockPrefab : blankBlockPrefab, this.transform);
                _instantiatedBlocks.Add(go);
                if (FilledSquare(x, y)) _instantiatedFilledBlocks.Add(go.GetComponent<Image>());
            }
        }

        _lightColor = _instantiatedFilledBlocks[0].color;
        _darkColor  = new Color(
                1f - _lightColor.r, 
                1f - _lightColor.g, 
                1f - _lightColor.b,  
                46f/255f);
        gameObject.SetActive(false);
        
        bool FilledSquare(int x, int y) => (x + y) % 2 == 0;
    }

    private void DarkModeColor(bool dark) {
        Color color = dark ? _darkColor: _lightColor;
        for (int i = 0; i < _instantiatedFilledBlocks.Count; i++)
            _instantiatedFilledBlocks[i].color = color;
    }

    private void DestroyOldBlocks() {
        for (int i = 0; i < _instantiatedBlocks.Count; i++)
            Destroy(_instantiatedBlocks[i]);
        
        _instantiatedFilledBlocks.Clear();
        _instantiatedBlocks.Clear();
    }

    public void ShowBlocks() => gameObject.SetActive(true);
}
