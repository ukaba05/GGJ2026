using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour, IDamageable
{
    [SerializeField]
    float _speed;

    [SerializeField]
    Rigidbody2D _rigidbody;

    [SerializeField]
    CinemachineVirtualCamera _camera;

    [SerializeField]
    ParticleSystem _particle;

    public bool IsInvincible { get; private set; }

    void FixedUpdate() {
        var direction = MapInputToDirection();
        _rigidbody.velocity = direction * _speed;
    }

    static Vector2 MapInputToDirection() {
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

    public void Damage() {
        Instantiate(_particle, transform.position, Quaternion.identity);
        SceneManager.LoadScene(1);
    }

    public void Invincible() {
        _rigidbody.excludeLayers = 1 << LayerMask.NameToLayer("Enemy");
        IsInvincible             = true;
    }

    public void Uninvincible() {
        _rigidbody.excludeLayers = 0;
        IsInvincible             = false;
    }
}