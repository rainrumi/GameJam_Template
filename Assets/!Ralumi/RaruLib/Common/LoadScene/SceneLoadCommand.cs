using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaruLib
{
    public class SceneLoadCommand : MonoBehaviour
    {
        private bool activeSceneLoad => SceneLoad.Instance != null;

        public void SceneLoad_Default(string name)
        {
            SceneManager.LoadScene(name);
        }

        public void SceneLoad_Fade(string name)
        {
            if (!activeSceneLoad) { Debug.Log("SceneLoad‚ª‚È‚¢", gameObject); return; }
            SceneLoad.Instance.SceneLoad_Fade(name).Forget();
        }
    }
}