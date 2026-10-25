using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameLoopManager : MonoBehaviour
{
    [Header("UI Прогресу Еволюції")]
    public TMP_Text populationText;     
    public Slider evolutionProgressBar; 

    [Header("Екрани Перемоги та Поразки")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Назва сцени Головного Меню")]
    public string mainMenuSceneName = "MainMenu"; // Впиши сюди точну назву своєї сцени меню

    private bool gameEnded = false;

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (evolutionProgressBar != null) evolutionProgressBar.maxValue = 200;
    }

    private void Update()
    {
        if (gameEnded) return;

        int playerUnits = 0;
        int playerNodes = 0;
        int enemyNodes = 0;

        foreach (var node in AltarNode.AllNodes)
        {
            if (node.owner == NodeOwner.Player)
            {
                playerNodes++;
                playerUnits += node.unitsCount;
            }
            else if (node.owner == NodeOwner.Enemy)
            {
                enemyNodes++;
            }
        }

        // 1. Англійський текст прогресу до Т2, Т3 та Левіафана
        if (populationText != null)
        {
            string goal = playerUnits < 20 ? "Next: Tier 2 (20)" :
                         (playerUnits < 100 ? "Next: Tier 3 (100)" :
                         (!AltarNode.BossSpawned ? "AWAKEN LEVIATHAN (200)" : "LEVIATHAN AWAKENED!"));

            populationText.text = $"Blaze Swarm: {playerUnits} | {goal}";
        }

        if (evolutionProgressBar != null)
        {
            evolutionProgressBar.value = playerUnits;
        }

        // 2. Перевірка Перемоги та Поразки
        if (enemyNodes == 0 && AltarNode.AllNodes.Count > 1)
        {
            EndGame(true);
        }
        else if (playerNodes == 0)
        {
            EndGame(false);
        }
    }

    private void EndGame(bool isWin)
    {
        gameEnded = true;
        if (isWin && winPanel != null) winPanel.SetActive(true);
        if (!isWin && losePanel != null) losePanel.SetActive(true);
    }

    // Метод для кнопки Restart
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Метод для кнопки Main Menu (на WinPanel та LosePanel)
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}