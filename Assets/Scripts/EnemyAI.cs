using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Налаштування ШІ")]
    public float actionInterval = 3.5f; // Раз на скільки секунд ворог робить хід
    public int minUnitsToAttack = 14;   // Мінімум юнітів у гнізді для атаки

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= actionInterval)
        {
            timer = 0f;
            MakeEnemyMove();
        }
    }

    private void MakeEnemyMove()
    {
        // Шукаємо всі ворожі гнізда, які накопичили достатньо сил
        List<AltarNode> readyEnemyNodes = new List<AltarNode>();
        foreach (var node in AltarNode.AllNodes)
        {
            if (node.owner == NodeOwner.Enemy && node.unitsCount >= minUnitsToAttack)
            {
                readyEnemyNodes.Add(node);
            }
        }

        if (readyEnemyNodes.Count == 0) return;

        // Вибираємо випадкове готове гніздо ворога
        AltarNode attacker = readyEnemyNodes[Random.Range(0, readyEnemyNodes.Count)];
        AltarNode bestTarget = FindTargetFor(attacker);

        if (bestTarget != null)
        {
            attacker.SendUnitsTo(bestTarget);
        }
    }

    private AltarNode FindTargetFor(AltarNode attacker)
    {
        AltarNode bestTarget = null;
        float bestScore = Mathf.Infinity;

        foreach (var node in AltarNode.AllNodes)
        {
            if (node.owner == NodeOwner.Enemy) continue; // Своїх не б'ємо

            float dist = Vector2.Distance(attacker.transform.position, node.transform.position);
            // Чим менше в гнізді захисників і чим воно ближче — тим краща ціль для ШІ
            float score = dist * 2f + node.unitsCount;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = node;
            }
        }
        return bestTarget;
    }
}