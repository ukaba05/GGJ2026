using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool isHorizontal = true; // true = x軸移動, false = y軸移動
    private int moveDirection = 1; // 1 或 -1

    [Header("偵測設定")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    private LayerMask obstacleLayer; // 障礙物圖層

    [Header("攻擊視錐設定")]
    [SerializeField] private float visionDistance = 5f;

    [Header("視線顯示設定")]
    [SerializeField] private bool showVisionLine = true; // 是否顯示視線
    [SerializeField] private Color normalVisionColor = Color.yellow; // 正常視線顏色
    [SerializeField] private float lineWidth = 0.05f; // 線條寬度

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
        // 設定 LineRenderer
        SetupLineRenderer();
    }

    void Update()
    {
        // 檢測前方是否有牆壁
        CheckWall();

        // 檢測視錐範圍內的玩家並攻擊
        DetectAndAttackPlayer();
    }

    void FixedUpdate()
    {
        // 移動
        Move();
    }

    void Move()
    {
        if (rb == null) return;

        if (isHorizontal)
        {
            // x 軸移動
            rb.velocity = new Vector2(moveDirection * moveSpeed, 0);
        }
        else
        {
            // y 軸移動
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
        // 根據移動軸向決定視線方向
        Vector2 direction;
        if (isHorizontal)
        {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // 射線檢測視錐範圍，直到碰到障礙物或玩家
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionDistance, obstacleLayer | (1 << LayerMask.NameToLayer("Player")));

        bool foundPlayer = false;

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                // 視線內有玩家，進行攻擊
                Debug.Log("Enemy Walk 發現玩家！攻擊距離: " + hit.distance);
                AttackPlayer(hit.collider.gameObject);
                foundPlayer = true;
            }
        }

        // 更新視線顯示
        UpdateVisionLine(direction, hit, foundPlayer);

        // 繪製視錐範圍（僅在 Scene 視圖中可見）
        float drawDistance = hit.collider != null ? hit.distance : visionDistance;
        Debug.DrawRay(transform.position, direction * drawDistance, foundPlayer ? Color.red : Color.yellow);
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

    // 在 Scene 視圖中繪製視錐範圍
    void OnDrawGizmosSelected()
    {
        Vector2 direction;
        if (isHorizontal)
        {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // 繪製視錐
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // 繪製牆壁檢測範圍
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);
    }

    void SetupLineRenderer()
    {
        // 檢查是否已有 LineRenderer，沒有則添加
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // 設定 LineRenderer 屬性
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = normalVisionColor;
        lineRenderer.endColor = normalVisionColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 10; // 確保在其他物件上方顯示

        // 如果不顯示視線，則禁用
        if (!showVisionLine)
        {
            lineRenderer.enabled = false;
        }
    }

    void UpdateVisionLine(Vector2 direction, RaycastHit2D hit, bool foundPlayer)
    {
        if (lineRenderer == null || !showVisionLine)
        {
            return;
        }

        // 確保 LineRenderer 啟用
        lineRenderer.enabled = true;

        // 設定起點
        lineRenderer.SetPosition(0, transform.position);

        // 設定終點（如果碰到東西，就畫到碰撞點；否則畫到最遠距離）
        Vector3 endPoint;
        if (hit.collider != null)
        {
            endPoint = hit.point;
        }
        else
        {
            endPoint = (Vector2)transform.position + direction * visionDistance;
        }
        lineRenderer.SetPosition(1, endPoint);
    }
}