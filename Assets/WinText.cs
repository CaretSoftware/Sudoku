using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class WinText : MonoBehaviour {
    public delegate void UpdateText(string text);
    public static UpdateText UpdateEndGameText;    
    [SerializeReference] private TextMeshProUGUI endGameText;
    [SerializeField, Range(0, 10)] private int msShowText = 1500;

    private void Awake() {
        TextActive(false);
        UpdateEndGameText = SetText;
    }

    private async void SetText(string text) {
        endGameText.text = text;
        TextActive(true);
        await Task.Delay(msShowText);
        TextActive(false);
    }

    private void TextActive(bool enabled) => gameObject.SetActive(enabled);

    private void OnDestroy() {
        UpdateEndGameText -= SetText;
    }
}
