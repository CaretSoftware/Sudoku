#if UNITY_STANDALONE_WIN
using System.Threading.Tasks;
using System.Threading;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AsyncLoading : MonoBehaviour {
#if UNITY_WEBGL
    private const float MSInSecond = 1000f;
#endif
#if UNITY_STANDALONE_WIN
    private CancellationTokenSource _cancellationTokenSource;
#endif
    private readonly Color _onColor = Color.white;
    private readonly Color _offColor = Color.black;
    private int _index;
    private int _maxIndex;
    private WaitForSeconds _waitForSeconds;
    
    [SerializeField] private int msDelay;
    [SerializeField] private Image[] images;
    
#if UNITY_STANDALONE_WIN
    private void Start() => Load();

    private async void Load() {
            _maxIndex = images.Length;
            while (true) {
                await Wait();
                if (images[_index] == null) return;
                images[_index].color = _onColor;
                _index++;
                
                if (_index != _maxIndex) continue;
                _index = 0;
                await Wait();
                ClearBar();
            }

            async Task Wait() {
                _cancellationTokenSource = new CancellationTokenSource();
                try {
                    await Task.Delay(msDelay, _cancellationTokenSource.Token);
                } catch { 
                    /*  Suppress exception when async continues it's execution
                        after gameObject has been destroyed */
                } finally {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
            
            void ClearBar() {
                if (images[_index] == null) return;
                for (int i = 0; i < _maxIndex; i++) {
                    images[i].color = _offColor;
                }
            }
        }
#endif

#if UNITY_WEBGL
    private void Awake() => _waitForSeconds = new WaitForSeconds(msDelay / MSInSecond);

    private void Start() => StartCoroutine(Load());

    private IEnumerator Load() {
        _maxIndex = images.Length;
        while (true) {
            yield return _waitForSeconds;
            images[_index].color = _onColor;
            _index++;
            
            if (_index != _maxIndex) continue;
            _index = 0;
            yield return _waitForSeconds;
            ClearBar();
        }
        
        void ClearBar() {
            for (int i = 0; i < _maxIndex; i++) {
                images[i].color = _offColor;
            }
        }
    }
#endif
}
