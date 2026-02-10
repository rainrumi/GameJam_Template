using UnityEngine;

namespace RaruLib
{
    [RequireComponent(typeof(SoundCommand))]
    public class TitleInitialize : MonoBehaviour
    {
        private SoundCommand _soundCommand;

        private void Start()
        {
            _soundCommand = GetComponent<SoundCommand>();
            //_soundCommand.CallStopBGM("main");
        }
    }
}