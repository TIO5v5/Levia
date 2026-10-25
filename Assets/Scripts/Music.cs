using UnityEngine;

public class MusicPersist : MonoBehaviour
{
    private static MusicPersist instance;

    private void Awake()
    {
        // Якщо плеєр вже існує (наприклад, ми повернулися з гри назад у меню),
        // знищуємо дублікат, щоб не грало дві музики одночасно!
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // Цей рядок переносить об'єкт з музикою у всі наступні сцени
        DontDestroyOnLoad(gameObject);
    }
}