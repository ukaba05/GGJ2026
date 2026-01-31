using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    [Header("死亡效果設定")]
    [SerializeField] private bool showDeathLog = true; // 是否顯示死亡訊息
    [SerializeField] private float destroyDelay = 0f; // 延遲銷毀時間（可用於播放死亡動畫）

    // 玩家受到傷害時被調用
    public void TakeDamage()
    {
        if (showDeathLog)
        {
            Debug.Log("玩家被攻擊！玩家死亡！");
        }

        // 銷毀玩家物件
        Die();
    }

    void Die()
    {
       Destroy(gameObject);
    }
    // 可選：使用 Trigger 碰撞
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("玩家進入敵人範圍！玩家死亡！");
            TakeDamage();
        }
    }
}