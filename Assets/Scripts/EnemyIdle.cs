using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdle : MonoBehaviour
{
    [Header("攻擊視錐設定")]
    [SerializeField] private float detectionRadius = 3f; // 圓形偵測範圍半徑

    [Header("視線顯示設定")]
    [SerializeField] private bool showDetectionCircle = true; // 是否顯示偵測圈
    [SerializeField] private Color normalCircleColor = new Color(0, 1, 1, 0.8f); // 正常圓圈顏色
    [SerializeField] private Color attackCircleColor = new Color(1, 0, 0, 0.9f); // 攻擊時圓圈顏色
    [SerializeField] private int circleSegments = 50; // 圓圈精細度
    [SerializeField] private float lineWidth = 0.15f; // 圓圈線條寬度

    private Animator animator;
    private LineRenderer lineRenderer;

    void Start()
    {
        // 設定 LineRenderer 繪製圓圈
        SetupCircleRenderer();
    }

    void Update()
    {
        // 偵測圓形範圍內的玩家並攻擊
        DetectAndAttackPlayer();
    }

    void DetectAndAttackPlayer()
    {
        // 使用 OverlapCircle 檢測圓形範圍內的所有碰撞體
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        bool foundPlayer = false;

        if (hitColliders != null && hitColliders.Length > 0)
        {
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider != null && hitCollider.CompareTag("Player"))
                {
                    // 偵測到玩家，進行攻擊
                    Debug.Log("Enemy Idle 發現玩家！實際偵測半徑: " + detectionRadius);
                    AttackPlayer(hitCollider.gameObject);
                    foundPlayer = true;
                    break;
                }
            }
        }

        // 更新圓圈顯示
        UpdateCircleDisplay(foundPlayer);
    }

    void AttackPlayer(GameObject player)
    {
        if (player == null) return;

        Player Damage = player.GetComponent<Player>();
        if (Damage != null)
        {
            Debug.Log("Enemy Rotate 攻擊玩家！");
            Damage.TakeDamage();
        }
    }

    void SetupCircleRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            Debug.LogError("無法創建 LineRenderer！");
            return;
        }

        // 設定 LineRenderer 屬性
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.useWorldSpace = true; // 改用世界空間，更準確
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor = normalCircleColor;
        lineRenderer.endColor = normalCircleColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 10;

        // 繪製圓形
        DrawCircle();

        if (!showDetectionCircle)
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawCircle()
    {
        if (lineRenderer == null) return;

        float deltaTheta = (2f * Mathf.PI) / circleSegments;
        float theta = 0f;

        for (int i = 0; i <= circleSegments; i++)
        {
            // 使用世界空間座標
            float x = transform.position.x + detectionRadius * Mathf.Cos(theta);
            float y = transform.position.y + detectionRadius * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, y, transform.position.z);
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    void UpdateCircleDisplay(bool foundPlayer)
    {
        if (!showDetectionCircle)
        {
            return;
        }

        // 每幀更新圓圈位置（因為敵人可能會移動）
        DrawCircle();

        // 更新顏色
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            Color currentColor = foundPlayer ? attackCircleColor : normalCircleColor;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
        }
    }

    // 在 Scene 視圖中繪製偵測範圍
    void OnDrawGizmosSelected()
    {
        // 繪製圓形偵測範圍 - 這個應該和實際偵測範圍完全一致
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}