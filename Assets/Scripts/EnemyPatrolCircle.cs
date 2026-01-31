using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolCircle : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool isHorizontal = true; // true = x軸移動, false = y軸移動
    private int moveDirection = 1; // 1 或 -1

    [Header("偵測設定")]
    [SerializeField] private float wallCheckDistance = 1f;
    private LayerMask obstacleLayer; // 障礙物圖層

    [Header("圓形攻擊設定")]
    [SerializeField] private float detectionRadius = 3f; // 圓形偵測範圍半徑
    [SerializeField] private float attackCooldown = 1f; // 攻擊冷卻時間
    private float lastAttackTime = 0f;

    [Header("視線顯示設定")]
    [SerializeField] private bool showDetectionCircle = true; // 是否顯示偵測圈
    [SerializeField] private Color circleColor = new Color(0, 1, 1, 0.8f); // 圓圈顏色（青色）
    [SerializeField] private int circleSegments = 50; // 圓圈精細度
    [SerializeField] private float lineWidth = 0.15f; // 圓圈線條寬度

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 設定障礙物 Layer
        obstacleLayer = LayerMask.GetMask("obstacle");

        // 設定 Rigidbody2D 不受重力影響
        if (rb != null)
        {
            rb.gravityScale = 0;
        }

        // 設定圓圈顯示
        SetupCircleRenderer();
    }

    void Update()
    {
        // 檢測前方是否有牆壁
        CheckWall();

        // 檢測圓形範圍內的玩家並攻擊
        DetectAndAttackPlayer();
    }

    void FixedUpdate()
    {
        // 移動
        Move();
    }

    void Move()
    {
        if (rb == null)
        {
            return;
        }

        if (isHorizontal)
        {
            rb.velocity = new Vector2(moveDirection * moveSpeed, 0);
        }
        else
        {
            rb.velocity = new Vector2(0, moveDirection * moveSpeed);
        }
    }

    void CheckWall()
    {
        // 根據移動軸向決定檢測方向
        Vector2 direction;
        if (isHorizontal)
        {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // 射線檢測前方是否有障礙物（牆壁或邊界）
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // 碰到障礙物，轉向
            Flip();
        }

        // 繪製偵測射線（僅在 Scene 視圖中可見）
        Debug.DrawRay(transform.position, direction * wallCheckDistance, Color.red);
    }

    void Flip()
    {
        // 反轉移動方向
        moveDirection *= -1;

        // 如果是水平移動，翻轉圖層（Sprite）
        if (isHorizontal && spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // 如果是垂直移動，可以選擇翻轉 Y 軸（可選）
        if (!isHorizontal && spriteRenderer != null)
        {
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }

    void DetectAndAttackPlayer()
    {
        // 使用 OverlapCircle 檢測圓形範圍內的所有碰撞體
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        if (hitColliders != null && hitColliders.Length > 0)
        {
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider != null && hitCollider.CompareTag("Player"))
                {
                    // 偵測到玩家，進行攻擊
                    Debug.Log("Enemy Patrol Circle 發現玩家！");
                    AttackPlayer(hitCollider.gameObject);
                    break;
                }
            }
        }

        // 更新圓圈顯示
        UpdateCircleDisplay();
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
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
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

    void UpdateCircleDisplay()
    {
        if (!showDetectionCircle)
        {
            return;
        }

        // 每幀更新圓圈位置（因為敵人在移動）
        DrawCircle();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    // 在 Scene 視圖中繪製偵測範圍
    void OnDrawGizmosSelected()
    {
        // 根據移動軸向決定檢測方向（顯示牆壁檢測）
        Vector2 direction;
        if (isHorizontal)
        {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // 繪製牆壁檢測範圍
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);

        // 繪製圓形偵測範圍
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}