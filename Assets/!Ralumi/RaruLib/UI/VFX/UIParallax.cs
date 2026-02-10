using DG.Tweening;
using UnityEngine;

namespace RaruLib
{
    public class UIParallax : MonoBehaviour
    {
        [Header("設定：数値が大きいほど大きく動く")]
        [SerializeField] private float weightX = 7f; // 横の重み
        [SerializeField] private float weightY = 5f; // 縦の重み
        [Header("詳細設定")]
        [SerializeField, Header("移動方向反転")] private bool reverse = false;

        private RectTransform _rectTransform;
        private Vector2 _initialPosition;

        void Start()
        {
            // ImageコンポーネントはRequireComponentで保証
            _rectTransform = GetComponent<RectTransform>();

            // 開始時のポジションを保持
            _initialPosition = _rectTransform.anchoredPosition;
        }

        void Update()
        {
            // マウスのスクリーン座標を取得 (0,0) 〜 (Screen.width, Screen.height)
            Vector2 mousePos = Input.mousePosition;
            if (reverse) mousePos = -mousePos;

            // 画面の中心を(0,0)とした -1.0 〜 1.0 の割合に変換
            float percentX = (mousePos.x / Screen.width) * 2f - 1f;
            float percentY = (mousePos.y / Screen.height) * 2f - 1f;

            // 重みを掛けて移動量を計算
            float offsetX = percentX * weightX;
            float offsetY = percentY * weightY;

            // 初期位置にオフセットを加えて適用
            _rectTransform.DOAnchorPos(
                new Vector2(
                    _initialPosition.x + offsetX,
                    _initialPosition.y + offsetY
                ),
                Time.deltaTime
            ).SetEase(Ease.OutCubic);
        }
    }
}