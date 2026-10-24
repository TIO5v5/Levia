using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameLoopManager : MonoBehaviour
{
    [Header("UI Прогресу Еволюції")]
    public TMP_Text populationText;     // Текст зверху екрана: "Популяція Блейза: 45 / 200"
    public Slider evolutionProgressBar; // Смужка прогресу до Боса (Max Value = 200)

    [Header("Екрани Перемоги та Поразки")]
    public GameObject winPanel;
    public GameObject losePanel;

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

        // 1. Оновлюємо верхній UI прогресу
        if (populationText != null)
        {
            string goal = playerUnits < 20 ? "До Т2 (Атака ворогів): 20" :
                         (playerUnits < 100 ? "До Т3 (Хижаки): 100" :
                         (!AltarNode.BossSpawned ? "До Пробудження ЛЕВІАФАНА: 200" : "ЛЕВІАФАН НА ВОЛІ!"));

            populationText.text = $"Блейзи: {playerUnits} | {goal}";
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

    // Прив'яжи ці методи до кнопок "Restart" або "Menu" на панелях перемоги/поразки
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}