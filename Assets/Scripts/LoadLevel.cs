using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Ezereal
{
    public class LoadLevel : MonoBehaviour // Restart Current Scene. Used in Demo scene.
    {
        public string sceneName;
        public void loadScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

