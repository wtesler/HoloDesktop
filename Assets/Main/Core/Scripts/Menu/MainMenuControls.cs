using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControls : MonoBehaviour
{
    public void StartDesktopScene() {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}