using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;

    private int number;
    public int Number {
        get => number;
        set {
            number = value;
            text.text = number.ToString();
        }
    }

    public CellManager cellManager { get; set; }

    public void Click() => cellManager.ClickedTile(Number);

    private void OnDestroy() => Clear();

    private void Clear() {
        background.color = Color.clear;
        text.text = string.Empty;
    }
}
