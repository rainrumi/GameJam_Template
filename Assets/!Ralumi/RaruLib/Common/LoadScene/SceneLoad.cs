using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RaruLib
{
    public class SceneLoad : MonoBehaviour
    {
        public static SceneLoad Instance { get; private set; }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private Image _image;
        [SerializeField] private float duration = 1.0f;

        // ロードが開始した時のイベント
        private Subject<Unit> LoadStartSubject = new Subject<Unit>();
        public IObservable<Unit> OnLoadStart => LoadStartSubject;

        // ロードが終了した時のイベント
        private Subject<Unit> LoadFinishSubject = new Subject<Unit>();
        public IObservable<Unit> OnLoadFinish => LoadFinishSubject;

        private void Awake()
        {
            if (Instance != null) { Debug.Log("SceneLoadが重複しています"); Destroy(gameObject); return; }
            Instance = this;

            _canvas.enabled = false;
        }

        public async UniTask SceneLoad_Fade(string name)
        {
            var halfDuration = duration / 2;
            var color = _image.color;
            color.a = 0;

            LoadStartSubject.OnNext(Unit.Default);

            _canvas.enabled = true;
            await UniTask.Yield();

            _image.DOComplete();
            _image.DOFade(1f, halfDuration).SetEase(Ease.OutQuad);
            await UniTask.WaitForSeconds(halfDuration);

            SceneManager.LoadScene(name);

            _image.DOComplete();
            _image.DOFade(0f, halfDuration).SetEase(Ease.OutQuad);
            await UniTask.WaitForSeconds(halfDuration);

            _canvas.enabled = false;
            await UniTask.Yield();

            LoadFinishSubject.OnNext(Unit.Default);
        }
    }
}