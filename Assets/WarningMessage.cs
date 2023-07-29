using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WarningMessage : MonoBehaviour {
    public delegate void WarningMessageDelegate(string message);
    public static WarningMessageDelegate warningMessage;

    [SerializeField, Range(0f, 1f)] private float popUpDuration = .5f;
    [SerializeField, Range(0f, 1f)] private float closeDuration = .25f;
    [SerializeField, Range(0f, 1f)] private float fadeFraction = .05f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform messageBoxRectTransform;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float secondsUntilClose = 2f;

    private enum WindowState {
        Closed,
        Opening,
        Open,
        Closing,
    }
    private WindowState _windowState = WindowState.Closed;
    private Coroutine _popupCoroutine;
    private string _message;
    
    private void Awake() => warningMessage += Warning;

    private void OnDestroy() => warningMessage -= Warning;

    [ContextMenu("WARNING")]
    private void Warn() => Warning("TEST");
    private void Warning(string message) {
        _message = message;
        Window(openWindow: true);
        //StartCoroutine(ShowWarning(message));
    }

    public void Close() {
        Window(openWindow: false);
    }

    private void Window(bool openWindow) {
        switch (_windowState) {
            case WindowState.Closed:
                if (openWindow) {
                    _popupCoroutine = StartCoroutine(MessagePopup());
                    StartCoroutine(CloseDelay(popUpDuration + secondsUntilClose));
                }
                break;
            case WindowState.Opening:
            case WindowState.Open:
                if (openWindow) break;
                if (_popupCoroutine != null)
                    StopCoroutine(_popupCoroutine);
                StartCoroutine(ClosePopup());
                break;
            case WindowState.Closing:
                break;
        }
    }

    public IEnumerator CloseDelay(float secondsUntilClose) {
        float t = 0f;
        do {
            t += Time.deltaTime;
            if (_windowState is WindowState.Closing or WindowState.Closing)
                yield break;
            yield return null;
        } while (t < secondsUntilClose);

        Window(openWindow: false);
    }

    public IEnumerator MessagePopup() {
        _windowState = WindowState.Opening;
        float t = 0f;
        
        do {
            t += Time.deltaTime * (1f / popUpDuration);
            float fadeProgress = Mathf.InverseLerp(0f, fadeFraction, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeProgress);
            messageBoxRectTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, Ease.OutElastic(t));
            
            if (_windowState == WindowState.Closing)
                yield break;
            
            yield return null;
        } while (t <= 1f);
        
        canvasGroup.alpha = 1f;
        messageBoxRectTransform.localScale = Vector3.one;
        _windowState = WindowState.Open;
    }
    
    public IEnumerator ClosePopup() {
        _windowState = WindowState.Closing;
        float t = 0f;
        Vector3 currentScale = messageBoxRectTransform.localScale;

        do {
            t += Time.deltaTime * (1f / closeDuration);
            float fadeProgress = Mathf.InverseLerp(0f, fadeFraction, 1f - t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeProgress);
            messageBoxRectTransform.localScale = Vector3.LerpUnclamped(currentScale, Vector3.zero, Ease.InExpo(t));
            yield return null;
        } while (t <= 1f);

        canvasGroup.alpha = 0f;
        messageBoxRectTransform.localScale = Vector3.zero;
        _windowState = WindowState.Closed;
    }
}
