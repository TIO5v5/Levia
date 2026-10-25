using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Назва твоєї ігрової сцени")]
    public string gameSceneName = "SampleScene"; // Заміни на точну назву сцени з грою!

    // Цей метод ми повісимо на кнопку PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Додатково: метод для кнопки EXIT (якщо захочеш додати вихід)
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}