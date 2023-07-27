using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private Color validColor;
    [SerializeField] private Color interactableColor;
    private string _numberString;
    private int _number;
    private bool _interactable = true;
    private bool _valid = true;
    
    public int Number {
        get => _number;
        set {
            _number = value;
            _numberString = _number.ToString();
            text.text = _numberString;
        }
    }
    public CellManager MyCellManager { get; set; }

    public void Click() {
        if (_interactable)
            MyCellManager.ClickedTile(Number);
    } 

    public void Hint(bool show) =>
        background.color = show && _valid ? validColor : Color.clear;

    public void Clear(bool clear) {
        _interactable = !clear;
        background.color = clear ? Color.clear : interactableColor;
        text.text = clear ? string.Empty : _numberString;
    }

    public void Valid(bool valid) {
        _valid = valid;
    }
}
