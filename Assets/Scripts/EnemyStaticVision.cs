using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStaticVision : MonoBehaviour
{
    [Header("視錐攻擊設定")]
    [SerializeField] private float visionDistance = 5f; // 視線距離
    [SerializeField] private VisionDirection visionDir = VisionDirection.Right; // 視線方向
    [SerializeField] private float attackCooldown = 1f; // 攻擊冷卻時間
    private float lastAttackTime = 0f;

    [Header("視線顯示設定")]
    [SerializeField] private bool showVisionLine = true;
    [SerializeField] private Color visionColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.1f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;

    // 視線方向選項
    public enum VisionDirection
    {
        Right,  // 向右 (1, 0)
        Left,   // 向左 (-1, 0)
        Up,     // 向上 (0, 1)
        Down    // 向下 (0, -1)
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 播放 idle 動畫
        if (animator != null)
        {
            animator.Play("Enemy idle");
        }

        // 根據視線方向翻轉 Sprite
        FlipSpriteBasedOnDirection();

        // 設定 LineRenderer
        SetupLineRenderer();
    }

    void Update()
    {
        // 檢測視錐範圍內的玩家並攻擊
        DetectAndAttackPlayer();
    }

    void FlipSpriteBasedOnDirection()
    {
        if (spriteRenderer == null) return;

        // 根據視線方向翻轉圖像
        switch (visionDir)
        {
            case VisionDirection.Left:
                spriteRenderer.flipX = true;
                break;
            case VisionDirection.Right:
                spriteRenderer.flipX = false;
                break;
            case VisionDirection.Up:
                // 可選：旋轉角色朝上
                break;
            case VisionDirection.Down:
                // 可選：旋轉角色朝下
                break;
        }
    }

    Vector2 GetVisionDirectionVector()
    {
        switch (visionDir)
        {
            case VisionDirection.Right:
                return Vector2.right;
            case VisionDirection.Left:
                return Vector2.left;
            case VisionDirection.Up:
                return Vector2.up;
            case VisionDirection.Down:
                return Vector2.down;
            default:
                return Vector2.right;
        }
    }

    void DetectAndAttackPlayer()
    {
        Vector2 direction = GetVisionDirectionVector();

        // 創建包含障礙物和玩家的 LayerMask
        LayerMask obstacleLayer = LayerMask.GetMask("obstacle");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        LayerMask combinedMask = obstacleLayer;

        if (playerLayerIndex >= 0)
        {
            combinedMask |= (1 << playerLayerIndex);
        }

        // 射線檢測視錐範圍
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionDistance, combinedMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // 視線內有玩家，進行攻擊
            Debug.Log("Enemy Static Vision 發現玩家！攻擊距離: " + hit.distance);
            AttackPlayer(hit.collider.gameObject);
        }

        // 更新視線顯示
        UpdateVisionLine(direction, hit);

        // Debug 繪製
        float drawDistance = hit.collider != null ? hit.distance : visionDistance;
        Debug.DrawRay(transform.position, direction * drawDistance, Color.yellow);
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

    void SetupLineRenderer()
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
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor = visionColor;
        lineRenderer.endColor = visionColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 10;

        if (!showVisionLine)
        {
            lineRenderer.enabled = false;
        }
    }

    void UpdateVisionLine(Vector2 direction, RaycastHit2D hit)
    {
        if (lineRenderer == null || !showVisionLine)
        {
            return;
        }

        lineRenderer.enabled = true;

        // 設定起點
        lineRenderer.SetPosition(0, transform.position);

        // 設定終點
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

    // 在 Scene 視圖中繪製視錐範圍
    void OnDrawGizmosSelected()
    {
        Vector2 direction = GetVisionDirectionVector();

        // 繪製視錐
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // 繪製方向標記
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + (Vector3)(direction * visionDistance), 0.2f);
    }
}