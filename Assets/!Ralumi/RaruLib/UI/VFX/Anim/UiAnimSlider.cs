using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RaruLib
{
    public class UiAnimSlider : UiAnimation
    {
        [SerializeField] Slider slider;
        [SerializeField, Tooltip("スライダーの値が変わるとAnimatorのValueが変更されます(※メソッドChangeValueをSliderにセットしてください)")]
        protected bool isHandleChange = false;

        public virtual void ChangeValue(float value)
        {
            if (isHandleChange) animator.SetFloat("Value", value);
        }

        private void OnEnable()
        {
            if (slider == null) { Debug.Log("sliderがアタッチされていません"); return; }
            if (isHandleChange) animator.SetFloat("Value", slider.value);
        }
    }
}