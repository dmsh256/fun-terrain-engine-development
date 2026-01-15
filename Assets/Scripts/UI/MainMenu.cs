using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void WalkAround()
        {
            SceneManager.LoadScene("Scenes/Game/Scene");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    
        public void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}
