using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName;

    public void StartGame()
    {
        EventManager.ClearAll();
        SceneManager.LoadScene(gameSceneName);
    }
    public void BackToTitle()
    {
        EventManager.ClearAll();
        SceneManager.LoadScene(gameSceneName);
    }

    public void Credits()
    {
        EventManager.ClearAll();
        SceneManager.LoadScene(gameSceneName);
    }

    public void Level2()
    {
        EventManager.ClearAll();
        SceneManager.LoadScene(gameSceneName);
    }
    public void Level3()
    {
        EventManager.ClearAll();
        SceneManager.LoadScene(gameSceneName);
    }
    public void QuitGame()
    {
        EventManager.ClearAll();
        Debug.Log("El juego se está cerrando...");
        Application.Quit();
    }
}
