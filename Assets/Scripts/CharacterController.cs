using Cinemachine;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    float _speed;

    [SerializeField]
    Rigidbody2D _rigidbody;

    [SerializeField]
    CinemachineVirtualCamera _camera;
    
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

    public void SetCharacter(Rigidbody2D rigidbody2D) {
        _rigidbody     = rigidbody2D;
        _camera.Follow = rigidbody2D.transform;
    }
}   