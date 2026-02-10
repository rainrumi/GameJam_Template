using UnityEngine;

namespace RaruLib
{
    [RequireComponent(typeof(SoundCommand))]
    public class SetSound : MonoBehaviour
    {
        private SoundCommand command;

        [SerializeField] private SoundKind kind = SoundKind.SE;
        [SerializeField] private string name = "click";

        private void Start()
        {
            command = GetComponent<SoundCommand>();
        }

        public void CallSound()
        {
            switch (kind)
            {
                case SoundKind.BGM:
                    command.CallPlayBGM(name);
                    break;
                case SoundKind.SE:
                    command.CallPlaySE(name);
                    break;
            }
        }
    }
}