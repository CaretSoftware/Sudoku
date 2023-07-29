using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour {
    public delegate void HideInvalid(bool shouldHide);
    public static HideInvalid HideInvalidTile;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private Color interactableColor;
    [SerializeField] private RectTransform textRectTransform;
    public int Number {
        get => _number;
        set {
            _number = value;
            _numberString = _number.ToString();
            text.text = _numberString;
        }
    }
    
    public TileManager MyTileManager { get; set; }
    public bool Cleared {
        get => _cleared;
        set {
            _cleared = value;
            ShowTile();
        } 
    }
    public bool Valid {
        get => _valid;
        set {
            _valid = value; 
            ShowTile();
        }
    }

    private static bool _shouldHide;
    private bool _valid = true;
    private bool _cleared;
    private string _numberString;
    private int _number;

    private void Awake() => HideInvalidTile += Hide;

    public void SetSize(Vector2 size) => textRectTransform.sizeDelta = size;

    private void OnDestroy() => HideInvalidTile -= Hide;

    public void Clear(Command.FillNumber.AddTileChange addTileChange) {
        if (!Cleared)
            addTileChange?.Invoke(new Command.TileChange(this, prevValid: Valid, currValid: false));
    }
    
    public void Click() {
        if (Blank())
            return;
        MyTileManager.ClickedTile(Number);
    }

    private void Hide(bool shouldHide) {
        _shouldHide = shouldHide;
        ShowTile();
    }

    private void ShowTile() {
        background.color = Blank() ?  Color.clear : interactableColor;
        text.alpha =  Blank() ? 0f : 1f;
    }

    private bool Blank() => Cleared || (_shouldHide && !Valid);
}
