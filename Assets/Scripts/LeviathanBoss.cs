using System.Collections.Generic;
using UnityEngine;

public class LeviathanBoss : MonoBehaviour
{
    [Header("Спрайти Блейза-Левіафана")]
    public Sprite bodySprite;
    public Sprite tailSprite;
    public int bodySegmentsCount = 10;   // Скільки сегментів тулуба спавнити
    public float segmentDistance = 0.45f; // Відстань між сегментами (підбери під розмір спрайта)

    [Header("Рух та Атака")]
    public float moveSpeed = 4.5f;
    public float turnSpeed = 180f;
    public float slitherSpeed = 6f;      // Швидкість зміїного звивання
    public float slitherAmount = 25f;    // Амплітуда виляння головою

    private List<Transform> segments = new List<Transform>();
    private AltarNode targetEnemyNode;
    private Vector3 manualTargetPos;
    private bool hasManualTarget = false;

    private void Start()
    {
        // Голова — це сам об'єкт, на якому висить скрипт
        SpriteRenderer headSr = GetComponent<SpriteRenderer>();
        if (headSr != null) headSr.sortingOrder = bodySegmentsCount + 5;

        segments.Add(transform);

        // Автоматично генеруємо ланцюжок тулуба та хвіст
        for (int i = 0; i < bodySegmentsCount; i++)
        {
            GameObject segObj = new GameObject($"BossSegment_{i}");
            segObj.transform.position = transform.position - transform.up * (segmentDistance * (i + 1));

            SpriteRenderer sr = segObj.AddComponent<SpriteRenderer>();
            bool isTail = (i == bodySegmentsCount - 1);
            sr.sprite = isTail ? tailSprite : bodySprite;
            
            // Кожен наступний сегмент лежить на шар нижче за попередній (під ним)
            sr.sortingOrder = (bodySegmentsCount + 4) - i;

            // Плавно зменшуємо розмір сегментів ближче до хвоста (від 100% до 70%)
            float scale = Mathf.Lerp(1f, 0.7f, (float)i / bodySegmentsCount);
            segObj.transform.localScale = transform.localScale * scale;

            segments.Add(segObj.transform);
        }
    }

    private void Update()
    {
        // ПКМ по карті — можемо вручну вказати Левіафану, куди плисти!
        if (Input.GetMouseButtonDown(1))
        {
            manualTargetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            manualTargetPos.z = 0;
            hasManualTarget = true;
        }

        MoveHead();
        UpdateBodySegments();
        CheckCrushNodes();
    }

    private void MoveHead()
    {
        Vector3 destination = transform.position + transform.up;

        // 1. Якщо немає ручної цілі — шукаємо найближче вороже гніздо
        if (!hasManualTarget)
        {
            if (targetEnemyNode == null || targetEnemyNode.owner == NodeOwner.Player)
                targetEnemyNode = FindClosestEnemyNode();

            if (targetEnemyNode != null)
                destination = targetEnemyNode.transform.position;
        }
        else
        {
            destination = manualTargetPos;
            if (Vector2.Distance(transform.position, manualTargetPos) < 0.8f)
                hasManualTarget = false;
        }

        // 2. Поворот голови до цілі + синусоїдальне зміїне виляння
        Vector2 dir = (destination - transform.position).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // -90 бо спрайт дивиться вгору
        float slither = Mathf.Sin(Time.time * slitherSpeed) * slitherAmount;

        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle + slither);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        // 3. Рух вперед
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    private void UpdateBodySegments()
    {
        // Кожен сегмент тягнеться за попереднім і повертається в його бік
        for (int i = 1; i < segments.Count; i++)
        {
            Transform current = segments[i];
            Transform prev = segments[i - 1];

            Vector3 dirToPrev = prev.position - current.position;
            float dist = dirToPrev.magnitude;

            if (dist > 0.001f)
            {
                // Повертаємо сегмент спиною за попереднім
                float angle = Mathf.Atan2(dirToPrev.y, dirToPrev.x) * Mathf.Rad2Deg - 90f;
                current.rotation = Quaternion.Euler(0, 0, angle);

                // Тримаємо жорстку дистанцію, як хребет
                if (dist > segmentDistance)
                {
                    current.position = prev.position - dirToPrev.normalized * segmentDistance;
                }
            }
        }
    }

    // Коли Левіафан пропливає над ворожим вівтарем — він миттєво трощить і захоплює його!
    private void CheckCrushNodes()
    {
        foreach (var node in AltarNode.AllNodes)
        {
            if (node.owner != NodeOwner.Player && Vector2.Distance(transform.position, node.transform.position) < 0.8f)
            {
                node.owner = NodeOwner.Player;
                node.unitsCount = 15;
                node.UpdateVisuals();
                hasManualTarget = false;
            }
        }
    }

    private AltarNode FindClosestEnemyNode()
    {
        AltarNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (var node in AltarNode.AllNodes)
        {
            if (node.owner != NodeOwner.Player)
            {
                float d = Vector2.Distance(transform.position, node.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = node;
                }
            }
        }
        return closest;
    }
}