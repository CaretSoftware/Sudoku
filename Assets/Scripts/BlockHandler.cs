using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockHandler : MonoBehaviour {
    [SerializeField] private GameObject blankBlockPrefab;
    [SerializeField] private GameObject filledBlockPrefab;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private float sudokuPanelDimension = 628.1552f;
    
    private readonly List<GameObject> _instantiatedBlocks = new List<GameObject>();

    private void Awake() => SpawnCells(9);

    public void SpawnCells(int size) {
        DestroyOldBlocks();
        int length = Mathf.RoundToInt(Mathf.Sqrt(size));
        float blockDimension = sudokuPanelDimension / length;
        gridLayoutGroup.constraintCount = length;
        gridLayoutGroup.cellSize = new Vector2(blockDimension, blockDimension);

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < length; y++) {
                GameObject go;
                go = Instantiate(OddSquare(x, y) ? filledBlockPrefab : blankBlockPrefab, this.transform);
                _instantiatedBlocks.Add(go);
            }
        }

        bool OddSquare(int x, int y) => (x + y) % 2 == 0;
    }

    private void DestroyOldBlocks() {
        for (int i = 0; i < _instantiatedBlocks.Count; i++)
            Destroy(_instantiatedBlocks[i]);
        
        _instantiatedBlocks.Clear();
    }
}
