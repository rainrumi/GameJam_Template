using UnityEngine;

namespace RaruLib
{
    [RequireComponent(typeof(SoundCommand))]
    public class GameInitialize : MonoBehaviour
    {
        private SoundCommand _soundCommand;

        private void Start()
        {
            _soundCommand = GetComponent<SoundCommand>();
            //_soundCommand.CallPlayBGM("main");
        }
    }
}