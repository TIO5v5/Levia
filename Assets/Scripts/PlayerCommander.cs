using UnityEngine;

public class PlayerCommander : MonoBehaviour
{
    public LineRenderer dragLine;
    private AltarNode selectedNode;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        if (dragLine != null)
        {
            dragLine.positionCount = 2;
            dragLine.enabled = false;
        }
    }

    private void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Малюємо лінію від вибраного гнізда до курсора миші
        if (selectedNode != null && dragLine != null)
        {
            dragLine.enabled = true;
            dragLine.SetPosition(0, selectedNode.transform.position);
            dragLine.SetPosition(1, mousePos);
        }
        else if (dragLine != null)
        {
            dragLine.enabled = false;
        }

        // 1. Натиснули ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.TryGetComponent(out AltarNode clickedNode))
            {
                // Якщо у нас ще не вибрана база І ми клікнули на СВОЮ -> вибираємо її
                if (selectedNode == null && clickedNode.owner == NodeOwner.Player)
                {
                    selectedNode = clickedNode;
                }
                // Якщо база ВЖЕ була вибрана і ми клікнули на інше гніздо -> шлемо туди війська!
                else if (selectedNode != null && clickedNode != selectedNode)
                {
                    selectedNode.SendUnitsTo(clickedNode);
                    selectedNode = null;
                }
                // Якщо клікнули на іншу свою базу — перемикаємо вибір або шлемо підкріплення
                else if (selectedNode == clickedNode)
                {
                    selectedNode = null; // Скасування вибору повторним кліком
                }
            }
            else
            {
                // Клік у пусте місце — знімаємо виділення
                selectedNode = null;
            }
        }

        // 2. Відпустили ЛКМ (для тих, хто любить тягнути затиснутою мишкою)
        if (Input.GetMouseButtonUp(0) && selectedNode != null)
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.TryGetComponent(out AltarNode targetNode) && targetNode != selectedNode)
            {
                selectedNode.SendUnitsTo(targetNode);
                selectedNode = null;
            }
        }

        // 3. Швидка атака з УСІХ своїх гнізд одразу по ПКМ на ціль!
        if (Input.GetMouseButtonDown(1))
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.TryGetComponent(out AltarNode targetNode))
            {
                foreach (var node in AltarNode.AllNodes)
                {
                    if (node.owner == NodeOwner.Player && node != targetNode)
                    {
                        node.SendUnitsTo(targetNode);
                    }
                }
                selectedNode = null;
            }
        }
    }
}