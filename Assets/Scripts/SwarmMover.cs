using UnityEngine;

public class SwarmMover : MonoBehaviour
{
    public float baseSpeed = 3.5f;
    public float wiggleSpeed = 12f;     // Швидкість виляння хвостиком
    public float wiggleMagnitude = 15f; // Кут виляння

    private AltarNode target;
    private NodeOwner owner;
    private int stage;
    private int powerAmount;
    private float randomOffset;

    public void Init(AltarNode targetNode, NodeOwner sender, int evoStage, Sprite stageSprite, int amount)
    {
        target = targetNode;
        owner = sender;
        stage = evoStage;
        powerAmount = amount;
        randomOffset = Random.Range(0f, 10f);

        GetComponent<SpriteRenderer>().sprite = stageSprite;
        baseSpeed += Random.Range(-0.4f, 0.4f); // Щоб рій летів природно, а не синхронно
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Рух до цільового вівтаря
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, baseSpeed * Time.deltaTime);

        // 2. Поворот головою до цілі + процедурне виляння (імітація плавання!)
        Vector2 dir = (target.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // -90 якщо спрайт намальований головою ВГОРУ
        float wiggle = Mathf.Sin((Time.time + randomOffset) * wiggleSpeed) * wiggleMagnitude;
        transform.rotation = Quaternion.Euler(0, 0, baseAngle + wiggle);

        // 3. Перевірка на прибуття
        if (Vector2.Distance(transform.position, target.transform.position) < 0.25f)
        {
            target.ReceiveUnits(owner, stage, powerAmount);
            Destroy(gameObject);
        }
    }
}