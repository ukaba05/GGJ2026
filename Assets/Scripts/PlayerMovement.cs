using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 設定 Rigidbody2D 不受重力影響
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 防止旋轉
        }
    }

    void Update()
    {
        // 獲取 WASD 輸入
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭頭
        float moveY = Input.GetAxisRaw("Vertical");   // W/S 或 上/下箭頭

        moveInput = new Vector2(moveX, moveY).normalized; // 正規化向量，防止斜向移動過快

        // 翻轉角色圖像（根據水平移動方向）
        if (moveX > 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // 向右不翻轉
        }
        else if (moveX < 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = true; // 向左翻轉
        }
    }

    void FixedUpdate()
    {
        // 在 FixedUpdate 中處理物理移動
        if (rb != null)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }

    // 在 Scene 視圖中繪製玩家位置標記
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}