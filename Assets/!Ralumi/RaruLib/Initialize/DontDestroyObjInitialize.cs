using UnityEngine;

namespace RaruLib
{
    public class DontDestroyObjInitialize : MonoBehaviour
    {
        [SerializeField] private GameObject DontDestroyPrefab;

        private void Awake()
        {
            var name = DontDestroyPrefab.name;

            if (GameObject.Find(name) != null) return;

            GameObject obj = Instantiate(DontDestroyPrefab);
            obj.name = name;
        }
    }
}