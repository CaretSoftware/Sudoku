#if UNITY_STANDALONE_WIN
using System.Collections;
using System.Threading.Tasks;
#endif
using UnityEngine;
using TMPro;

public class WinText : MonoBehaviour {
    private const float MSInSecond = 1000;
    public delegate void UpdateText(string text);
    public static UpdateText UpdateEndGameText;    
    [SerializeReference] private TextMeshProUGUI endGameText;
    [SerializeField, Range(0, 10000)] private int msShowText = 1500;

    private void Awake() {
        TextActive(false);
        UpdateEndGameText = SetText;
    }
#if UNITY_STANDALONE_WIN
    private async void SetText(string text) {
        endGameText.text = text;
        TextActive(true);
        await Task.Delay(msShowText);
        TextActive(false);
    }
#endif
#if UNITY_WEBGL
    private void SetText(string text) {
        endGameText.text = text;
        TextActive(true);
        HideTextAfter(msShowText / MSInSecond);
    }

    private void HideTextAfter(float seconds) => Invoke(nameof(HideText), seconds);
    
    private void HideText() => TextActive(false);
#endif

    private void TextActive(bool enabled) => gameObject.SetActive(enabled);

    private void OnDestroy() => UpdateEndGameText -= SetText;
}
