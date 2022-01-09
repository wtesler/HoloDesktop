using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControls : MonoBehaviour
{
    public void StartDesktopScene() {
        SceneManager.LoadScene("DesktopScene", LoadSceneMode.Single);
    }

    public void StartVideoScene() {
        SceneManager.LoadScene("VideoScene", LoadSceneMode.Single);
    }

    public void StartGameScene() {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void StartWebcamScene() {
        SceneManager.LoadScene("WebcamScene", LoadSceneMode.Single);
    }
}
