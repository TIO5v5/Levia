using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NodeOwner { Player, Neutral, Enemy }

public class AltarNode : MonoBehaviour
{
    public static List<AltarNode> AllNodes = new List<AltarNode>();
    public static int GlobalPlayerStage = 0; // 0 = Т1, 1 = Т2 (20+), 2 = Т3 (100+)
    public static int GlobalEnemyStage = 0;  // Ворог теж качається від своєї популяції!
    public static bool BossSpawned = false;

    [Header("Налаштування вузла")]
    public NodeOwner owner = NodeOwner.Neutral;
    public bool isMainBase = false; // Галочка для головної бази (твоєї або ворожої)
    public int unitsCount = 5;
    public int maxCapacity = 50;

    [Header("Спрайти САМИХ ГНІЗД (Баз)")]
    public Sprite emptyNestSprite;     // Сюди закинь empty nest.png (сіре з оком)
    public Sprite playerNestSprite;    // Сюди закинь червоне гніздо Блейза з кристалом
    public Sprite[] enemyNestSprites;  // [0] = Enemy nest (1 корона), [1] = Enemy nest t2 (2 корони)

    [Header("Спрайти ЮНІТІВ (Твої Блейзи і Ворожі Блоби)")]
    public Sprite[] playerUnitSprites; // [0] = Т1 круглий, [1] = Т2 малий рогатий, [2] = Т3 великий
    public Sprite[] enemyUnitSprites;  // [0] = blob sosynok, [1] = blob t2, [2] = blob t3
    public GameObject leviathanBossPrefab;

    [Header("Візуал та UI")]
    public TMP_Text countText;
    public GameObject swarmUnitPrefab;

    private SpriteRenderer sr;
    private float spawnTimer;
    private Vector3 initialScale;

    private void OnEnable() => AllNodes.Add(this);
    private void OnDisable() => AllNodes.Remove(this);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
    }

    private void Start()
    {
        if (owner == NodeOwner.Player && isMainBase)
        {
            GlobalPlayerStage = 0;
            GlobalEnemyStage = 0;
            BossSpawned = false;
        }
        UpdateVisuals();
    }

    private void Update()
    {
        // Народжують тільки зайняті гнізда (Player та Enemy)
        if (owner != NodeOwner.Neutral && unitsCount < maxCapacity)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= 0.85f)
            {
                spawnTimer = 0f;
                unitsCount++;
                CheckGlobalEvolution();
                UpdateVisuals();
            }
        }
    }

    public static void CheckGlobalEvolution()
    {
        int totalPlayer = 0;
        int totalEnemy = 0;

        foreach (var node in AllNodes)
        {
            if (node.owner == NodeOwner.Player) totalPlayer += node.unitsCount;
            else if (node.owner == NodeOwner.Enemy) totalEnemy += node.unitsCount;
        }

        // 1. Еволюція Гравця (Блейза)
        int newPlayerStage = (totalPlayer >= 100) ? 2 : (totalPlayer >= 20 ? 1 : 0);
        // 2. Еволюція Ворога (Зелених Блобів)
        int newEnemyStage = (totalEnemy >= 80) ? 2 : (totalEnemy >= 20 ? 1 : 0);

        if (newPlayerStage != GlobalPlayerStage || newEnemyStage != GlobalEnemyStage)
        {
            GlobalPlayerStage = newPlayerStage;
            GlobalEnemyStage = newEnemyStage;
            foreach (var node in AllNodes)
                node.UpdateVisuals();
        }

        // Пробудження Боса Блейза при 200 популяції
        if (totalPlayer >= 200 && !BossSpawned)
        {
            BossSpawned = true;
            foreach (var node in AllNodes)
            {
                if (node.owner == NodeOwner.Player && node.isMainBase && node.leviathanBossPrefab != null)
                {
                    Instantiate(node.leviathanBossPrefab, node.transform.position, Quaternion.identity);
                    break;
                }
            }
        }
    }

    public void SendUnitsTo(AltarNode targetNode)
    {
        if (unitsCount <= 1 || targetNode == this) return;

        // Малюки Т1 можуть захоплювати тільки пусті гнізда (Neutral) або підкріплювати свої!
        if (owner == NodeOwner.Player && GlobalPlayerStage == 0 && targetNode.owner == NodeOwner.Enemy)
        {
            Debug.Log("Т1 можуть захоплювати тільки пусті сірі гнізда! Набери 20 Блейзів для Т2!");
            return;
        }

        int sendAmount = unitsCount / 2;
        unitsCount -= sendAmount;
        UpdateVisuals();

        // Визначаємо стадію і правильний спрайт (червоний Блейз чи зелений Блоб)
        int currentStage = (owner == NodeOwner.Player) ? GlobalPlayerStage : GlobalEnemyStage;
        Sprite[] activeArray = (owner == NodeOwner.Player) ? playerUnitSprites : enemyUnitSprites;
        Sprite unitSprite = activeArray[Mathf.Clamp(currentStage, 0, activeArray.Length - 1)];

        int visualSpritesCount = Mathf.Clamp(sendAmount, 1, 8);
        for (int i = 0; i < visualSpritesCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 0.45f;
            GameObject unitObj = Instantiate(swarmUnitPrefab, transform.position + randomOffset, Quaternion.identity);
            SwarmMover mover = unitObj.GetComponent<SwarmMover>();

            int power = (i == 0) ? sendAmount : 0;
            mover.Init(targetNode, owner, currentStage, unitSprite, power);
        }
    }

    public void ReceiveUnits(NodeOwner senderOwner, int senderStage, int amount)
    {
        if (amount == 0) return;

        int myStage = (owner == NodeOwner.Player) ? GlobalPlayerStage : 
                      (owner == NodeOwner.Enemy ? GlobalEnemyStage : 0);

        int attackMult = (senderStage == 0) ? 1 : (senderStage == 1 ? 2 : 4);
        int defenseMult = (myStage == 0) ? 1 : (myStage == 1 ? 2 : 4);

        if (senderOwner == owner)
        {
            unitsCount = Mathf.Min(unitsCount + amount, maxCapacity);
        }
        else
        {
            int resultPower = (unitsCount * defenseMult) - (amount * attackMult);
            if (resultPower < 0)
            {
                // Гніздо переходить новому власнику і саме змінює картинку!
                owner = senderOwner;
                unitsCount = Mathf.Max(1, Mathf.Abs(resultPower) / attackMult);
            }
            else
            {
                unitsCount = resultPower / defenseMult;
            }
        }

        CheckGlobalEvolution();
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        // 1. Міняємо спрайт самого гнізда залежно від того, чиє воно зараз!
        if (owner == NodeOwner.Neutral && emptyNestSprite != null)
        {
            sr.sprite = emptyNestSprite;
        }
        else if (owner == NodeOwner.Player && playerNestSprite != null)
        {
            sr.sprite = playerNestSprite;
        }
        else if (owner == NodeOwner.Enemy && enemyNestSprites != null && enemyNestSprites.Length > 0)
        {
            // Якщо ворог прокачався до Т2+ — ставимо спрайт з двома коронами (Enemy nest t2)!
            int enemyNestIndex = (GlobalEnemyStage >= 1 && enemyNestSprites.Length > 1) ? 1 : 0;
            sr.sprite = enemyNestSprites[enemyNestIndex];
        }

        // 2. Збільшуємо розмір головної бази з ростом тіру
        if (isMainBase && owner != NodeOwner.Neutral)
        {
            int stage = (owner == NodeOwner.Player) ? GlobalPlayerStage : GlobalEnemyStage;
            transform.localScale = initialScale * (1f + stage * 0.2f);
        }

        // 3. Оновлюємо текст над гніздом
        if (countText != null)
        {
            string info = (owner == NodeOwner.Player) ? $" [T{GlobalPlayerStage + 1}]" :
                          (owner == NodeOwner.Enemy ? $" [T{GlobalEnemyStage + 1}]" : "");
            countText.text = $"{unitsCount}/{maxCapacity}{info}";

            countText.color = (owner == NodeOwner.Player) ? new Color(1f, 0.35f, 0.35f) :
                              (owner == NodeOwner.Enemy ? new Color(0.3f, 1f, 0.3f) : Color.gray);
        }
    }
}