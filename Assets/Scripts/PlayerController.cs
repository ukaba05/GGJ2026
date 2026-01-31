using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

// 改為 PlayerController（或其他名稱）
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float _speed;
    [SerializeField]
    int isInvulnerablityTime = 1; // 改為 SerializeField 方便調整
    [SerializeField]
    Rigidbody2D _rigidbody;
    [SerializeField]
    CinemachineVirtualCamera _camera;
    [SerializeField]
    bool isInvulnerable = false;

    void FixedUpdate()
    {
        var direction = MapInputToDirection();
        _rigidbody.velocity = direction * _speed;
    }

    public void ActivateInvulnerability() // 改名：方法名不應該用 is 開頭
    {
        if (!isInvulnerable) // 防止重複觸發
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    static Vector2 MapInputToDirection()
    {
        var direction = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            direction += Vector2.up;
        if (Input.GetKey(KeyCode.A))
            direction += Vector2.left;
        if (Input.GetKey(KeyCode.S))
            direction += Vector2.down;
        if (Input.GetKey(KeyCode.D))
            direction += Vector2.right;
        return direction;
    }

    public void SetCharacter(Rigidbody2D rigidbody2D)
    {
        _rigidbody = rigidbody2D;
        _camera.Follow = rigidbody2D.transform;
    }

    IEnumerator InvulnerabilityCoroutine() // 改名：更清楚
    {
        isInvulnerable = true;
        gameObject.tag = "Enemy";
        Debug.Log("無敵狀態開始");

        yield return new WaitForSeconds(isInvulnerablityTime);

        gameObject.tag = "Player";
        isInvulnerable = false;
        Debug.Log("無敵狀態結束");
    }

    public void TakeDamage()
    {
        if (isInvulnerable)
        {
            Debug.Log("玩家處於無敵狀態，忽略傷害");
            return;
        }

        Debug.Log("玩家受到傷害，死亡");
        Destroy(gameObject);
    }
}