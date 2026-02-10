using UnityEngine;

namespace RaruLib
{
    public class JumpURL : MonoBehaviour
    {
        [SerializeField] string xUrl = "https://x.com";

        public void OpenX()
        {
            Application.OpenURL(xUrl);
        }
    }
}