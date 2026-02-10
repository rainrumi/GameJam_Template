using RaruLib;
using UnityEngine;
using UnityEngine.UI;

namespace RaruLib
{
    [RequireComponent(typeof(Slider))]
    public class SoundSliderInitialize : MonoBehaviour
    {
        private bool activeSound => Sound.instance != null;
        private Slider slider;
        [SerializeField] private SoundKind kind = SoundKind.BGM;

        private void Start()
        {
            if (!activeSound) { Debug.Log("Sound‚ª‚ ‚è‚Ü‚¹‚ñ", gameObject); return; }

            slider = GetComponent<Slider>();

            switch (kind)
            {
                case SoundKind.BGM:
                    slider.value = Sound.instance.GetVolume("BGM");
                    break;
                case SoundKind.SE:
                    slider.value = Sound.instance.GetVolume("SE");
                    break;
            }
        }
    }
}