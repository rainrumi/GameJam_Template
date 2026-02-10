using UnityEngine;
using UnityEngine.UI;

namespace RaruLib
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasInitialize : MonoBehaviour
    {
        private Canvas _canvas;
        private CanvasScaler _canvasScaler;

        [Header("キャンバスの設定")]
        [SerializeField] private RenderMode renderMode = RenderMode.ScreenSpaceCamera;
        [SerializeField] private bool useMainCamera = true;
        [Header("キャンバススカラーの設定")]
        [SerializeField] private CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        [SerializeField] private Vector2 scalerResolution = new Vector2(1920, 1080);

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            _canvasScaler = GetComponent<CanvasScaler>();

            _canvas.renderMode = renderMode;
            if (useMainCamera) _canvas.worldCamera = Camera.main;

            _canvasScaler.uiScaleMode = scaleMode;
            _canvasScaler.referenceResolution = scalerResolution;
        }
    }
}