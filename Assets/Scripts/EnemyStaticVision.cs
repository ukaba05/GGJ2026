using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStaticVision : MonoBehaviour, IISolationable
{
    [Header("���@�����]�w")]
    [SerializeField] private float visionDistance = 5f; // ���u�Z��

    [SerializeField] private VisionDirection visionDir      = VisionDirection.Right; // ���u��V
    [SerializeField] private float           attackCooldown = 1f;                    // �����N�o�ɶ�
    private                  float           lastAttackTime = 0f;

    [Header("���u��ܳ]�w")]
    [SerializeField] private bool showVisionLine = true;

    [SerializeField] private Color visionColor = Color.yellow;
    [SerializeField] private float lineWidth   = 0.1f;

    private Animator       animator;
    private SpriteRenderer spriteRenderer;
    private LineRenderer   lineRenderer;

    int _attackTarget;

    // ���u��V�ﶵ
    public enum VisionDirection
    {
        Right, // �V�k (1, 0)
        Left,  // �V�� (-1, 0)
        Up,    // �V�W (0, 1)
        Down   // �V�U (0, -1)
    }

    void Start() {
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ���� idle �ʵe
        if (animator != null) {
            animator.Play("Enemy idle");
        }

        // �ھڵ��u��V½�� Sprite
        FlipSpriteBasedOnDirection();

        // �]�w LineRenderer
        SetupLineRenderer();

        _attackTarget = 1 << LayerMask.NameToLayer("Player");
    }

    void Update() {
        // �˴����@�d�򤺪����a�ç���
        DetectAndAttackPlayer();
    }

    void FlipSpriteBasedOnDirection() {
        if (spriteRenderer == null) return;

        // �ھڵ��u��V½��Ϲ�
        switch (visionDir) {
            case VisionDirection.Left:
                spriteRenderer.flipX = true;

                break;
            case VisionDirection.Right:
                spriteRenderer.flipX = false;

                break;
            case VisionDirection.Up:
                // �i��G���ਤ��¤W
                break;
            case VisionDirection.Down:
                // �i��G���ਤ��¤U
                break;
        }
    }

    Vector2 GetVisionDirectionVector() {
        switch (visionDir) {
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

    void DetectAndAttackPlayer() {
        Vector2 direction = GetVisionDirectionVector();

        var origin = transform.position + (Vector3)direction;
        var allHits =
            Physics2D
                .RaycastAll(origin, direction, visionDistance, _attackTarget);


        bool foundPlayer = false;

        if (allHits != null && allHits.Length > 0) {
            foreach (var hit in allHits) {
                AttackPlayer(hit.transform.gameObject);
                Debug.Log("Enemy Static " + allHits.Length);
                foundPlayer = true;
            }
        }

        // ��s���u���
        UpdateVisionLine(direction);

        // Debug ø�s
        Debug.DrawRay(transform.position, direction * visionDistance, foundPlayer ? Color.red : Color.yellow);
    }

    void AttackPlayer(GameObject player) {
        if (player == null) return;

        // �ˬd�����N�o�ɶ�
        if (Time.time - lastAttackTime < attackCooldown) {
            return;
        }

        lastAttackTime = Time.time;
    }

    void SetupLineRenderer() {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (lineRenderer == null) {
            Debug.LogError("�L�k�Ы� LineRenderer�I");

            return;
        }

        // �]�w LineRenderer �ݩ�
        lineRenderer.startWidth    = lineWidth;
        lineRenderer.endWidth      = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled       = false;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null) {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor       = visionColor;
        lineRenderer.endColor         = visionColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder     = 10;

        if (!showVisionLine) {
            lineRenderer.enabled = false;
        }
    }

    void UpdateVisionLine(Vector2 direction) {
        if (lineRenderer == null || !showVisionLine) {
            return;
        }

        // �]�w�_�I
        lineRenderer.SetPosition(0, transform.position);

        // �]�w���I
        Vector3 endPoint = (Vector2)transform.position + direction * visionDistance;


        lineRenderer.SetPosition(1, endPoint);
    }

    // �b Scene ���Ϥ�ø�s���@�d��
    void OnDrawGizmosSelected() {
        Vector2 direction = GetVisionDirectionVector();

        // ø�s���@
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // ø�s��V�аO
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + (Vector3)(direction * visionDistance), 0.2f);
    }

    public void Isolation() {
        Debug.Log("test");
        _attackTarget |= 1 << LayerMask.NameToLayer("Enemy");
    }
}