using DG.Tweening;
using UnityEngine;

namespace RaruLib
{
    public class ShakeVFX : MonoBehaviour
    {
        [SerializeField] float duration = 1.5f;
        [SerializeField] float strength = 0.1f;
        [SerializeField] int vibrato = 10;
        [SerializeField] float randomness = 90f;

        Tween shakeTween;

        void Start()
        {
            shakeTween = transform.DOShakePosition(
                    duration,
                    strength,
                    vibrato,
                    randomness,
                    fadeOut: false
                )
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }

        void OnDestroy()
        {
            shakeTween?.Kill();
        }
    }
}