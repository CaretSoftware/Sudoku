using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour {
    public delegate void ShowValid(bool show);
    public static ShowValid ShowValidDelegate;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private Color interactableColor;
    
    public int Number {
        get => _number;
        set {
            _number = value;
            _numberString = _number.ToString();
            text.text = _numberString;
        }
    }
    public CellManager MyCellManager { get; set; }
    public bool Cleared { get; private set; }

    private bool _interactable = true;
    private bool _valid = true;
    private bool _show;
    private string _numberString;
    private int _number;

    private void Awake() => ShowValidDelegate += Show;

    private void OnDestroy() => ShowValidDelegate -= Show;

    public void Click() {
        if (_interactable)
            MyCellManager.ClickedTile(Number);
    }

    private void Show(bool hide) {
        _show = hide;
        if (Cleared) 
            return;
        background.color = hide && !_valid ?  Color.clear : interactableColor;
        text.alpha = hide && !_valid ? 0f : 1f;
    }

    public void Clear(bool clear, bool valid = true, bool assigned = false) {
        Cleared = assigned;
        _interactable = !assigned;
        _valid = valid;
        if (assigned) {
            background.color = clear ? Color.clear : interactableColor;
            text.alpha = clear ? 0f : 1f;
            return;
        }
        if (_show) 
            Show(true);
    }

    public void Valid(bool valid) => _valid = valid; // TODO Show(_show); ?
}

// TODO pass down Command.FillNumbers delegate '_addToTileChangesList' to the tiles that are checked for invalidness.
// TODO reduce the number of bools. keep Invalid. Set all tiles under filled cell as Invalid in command