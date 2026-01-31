using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotate : MonoBehaviour
{
    [Header("旋轉設定")]
    [SerializeField] private float rotationSpeed = 45f; // 每秒旋轉度數
    [SerializeField] private bool clockwise = true; // true = 順時針, false = 逆時針
    [SerializeField] private float startAngle = 0f; // 起始角度（0 = 向右）
    [SerializeField] private float sweepAngle = 360f; // 掃描角度範圍（360 = 完整圓圈）

    [Header("攻擊視錐設定")]
    [SerializeField] private float visionDistance = 5f; // 視線距離
    [SerializeField] private float attackCooldown = 1f; // 攻擊冷卻時間
    private float lastAttackTime = 0f;

    [Header("視線顯示設定")]
    [SerializeField] private bool showVisionLine = true;
    [SerializeField] private Color visionColor = Color.yellow; // 視線顏色
    [SerializeField] private float lineWidth = 0.05f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private float currentVisionAngle = 0f; // 視線當前角度

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 開始執行 idle 或 walk 動畫
        if (animator != null)
        {
            animator.Play("Enemy idle"); // 或 "Enemy walk"
        }

        // 設定 LineRenderer
        SetupLineRenderer();

        // 初始視線角度
        currentVisionAngle = startAngle;
    }

    void Update()
    {
        // 旋轉視線角度（不旋轉角色本身）
        RotateVision();

        // 檢測視錐範圍內的玩家並攻擊
        DetectAndAttackPlayer();
    }

    void RotateVision()
    {
        // 計算旋轉
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        if (!clockwise)
        {
            rotationThisFrame = -rotationThisFrame;
        }

        currentVisionAngle += rotationThisFrame;

        // 如果設定了掃描範圍限制（不是 360 度）
        if (sweepAngle < 360f)
        {
            // 在範圍內來回擺動
            float minAngle = startAngle;
            float maxAngle = startAngle + sweepAngle;

            if (currentVisionAngle > maxAngle || currentVisionAngle < minAngle)
            {
                // 反轉方向
                clockwise = !clockwise;
                currentVisionAngle = Mathf.Clamp(currentVisionAngle, minAngle, maxAngle);
            }
        }
    }

    void DetectAndAttackPlayer()
    {
        // 視線方向根據當前角度計算（不依賴物體旋轉）
        float angleInRadians = currentVisionAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // 創建包含障礙物和玩家的 LayerMask
        LayerMask obstacleLayer = LayerMask.GetMask("obstacle");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        LayerMask combinedMask = obstacleLayer;

        if (playerLayerIndex >= 0)
        {
            combinedMask |= (1 << playerLayerIndex);
        }

        // 射線檢測
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionDistance, combinedMask);

        bool foundPlayer = false;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.Log("Enemy Rotate 發現玩家！攻擊距離: " + hit.distance);
            AttackPlayer(hit.collider.gameObject);
        }

        // 更新視線顯示
        UpdateVisionLine(direction, hit);

        // Debug 繪製
        Debug.DrawRay(transform.position, direction * visionDistance, Color.yellow);
    }

    void AttackPlayer(GameObject player)
    {
        if (player == null) return;

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Debug.Log("Enemy Rotate 攻擊玩家！");
            playerHealth.TakeDamage();
        }
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (lineRenderer == null) return;

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

        // 起點
        lineRenderer.SetPosition(0, transform.position);

        // 終點
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

    void OnDrawGizmosSelected()
    {
        // 繪製視錐（使用視線角度）
        float angleInRadians = currentVisionAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // 如果有掃描範圍限制，繪製掃描範圍
        if (sweepAngle < 360f)
        {
            Gizmos.color = Color.cyan;
            float startRad = startAngle * Mathf.Deg2Rad;
            float endRad = (startAngle + sweepAngle) * Mathf.Deg2Rad;

            Vector2 startDir = new Vector2(Mathf.Cos(startRad), Mathf.Sin(startRad));
            Vector2 endDir = new Vector2(Mathf.Cos(endRad), Mathf.Sin(endRad));

            Gizmos.DrawRay(transform.position, startDir * visionDistance);
            Gizmos.DrawRay(transform.position, endDir * visionDistance);
        }
    }
}