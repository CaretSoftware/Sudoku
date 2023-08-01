using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AsyncLoading : MonoBehaviour {
    private int _index;
    private int _maxIndex;
    
    [SerializeField] private int msDelay;
    [SerializeField] private Image[] images;
    
    private void Start() => Load();

    private async void Load() {
        _maxIndex = images.Length;
        while (true) {
            await Task.Delay(msDelay);
            if (images[_index] == null) return;
            images[_index].color = Color.black;
            _index++;
            
            if (_index != _maxIndex) continue;
            _index = 0;
            await Task.Delay(msDelay);
            ClearBar();
        }
        
        void ClearBar() {
            if (images[_index] == null) return;
            for (int i = 0; i < _maxIndex; i++) {
                images[i].color = Color.clear;
            }
        }
    } 
}
