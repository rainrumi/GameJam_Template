using UnityEngine;

namespace RaruLib
{
    public class SoundSliderCommand : MonoBehaviour
    {
        private bool activeSound => Sound.instance != null;

        public void ChangeBgmVol(float value)
        {
            if (!activeSound) { Debug.Log("Sound‚ª‚ ‚è‚Ü‚¹‚ñ", gameObject); return; }
            Sound.instance.ChangeVolume("BGM", value);
        }

        public void ChangeSeVol(float value)
        {
            if (!activeSound) { Debug.Log("Sound‚ª‚ ‚è‚Ü‚¹‚ñ", gameObject); return; }
            Sound.instance.ChangeVolume("SE", value);
        }
    }
}