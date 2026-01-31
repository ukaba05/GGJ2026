using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPatrolCircle : MonoBehaviour, IIsolationable, IDamageable
{
    [Header("���ʳ]�w")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private bool isHorizontal  = true; // true = x�b����, false = y�b����
    private                  int  moveDirection = 1;    // 1 �� -1

    [Header("�����]�w")]
    [SerializeField] private float wallCheckDistance = 0.5f;

    private LayerMask obstacleLayer; // ��ê���ϼh

    [Header("��Χ����]�w")]
    [SerializeField] private float detectionRadius = 3f; // ��ΰ����d��b�|

    [SerializeField] private float attackCooldown = 1f; // �����N�o�ɶ�
    private                  float lastAttackTime = 0f;

    [Header("���u��ܳ]�w")]
    [SerializeField] private bool showDetectionCircle = true; // �O�_��ܰ�����

    [SerializeField] private Color circleColor    = new Color(0, 1, 1, 0.8f); // ����C��]�C��^
    [SerializeField] private int   circleSegments = 50;                       // ����ӫ�
    [SerializeField] private float lineWidth      = 0.15f;                    // ���u���e��

    [SerializeField]
    ParticleSystem _particle;

    private Rigidbody2D    rb;
    private SpriteRenderer spriteRenderer;
    private LineRenderer   lineRenderer;

    int _attackTarget;

    void Start() {
        rb             = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �]�w��ê�� Layer
        obstacleLayer = LayerMask.GetMask("obstacle");

        // �]�w Rigidbody2D �������O�v�T
        if (rb != null) {
            rb.gravityScale = 0;
        }

        // �]�w������
        SetupCircleRenderer();

        _attackTarget |= 1 << LayerMask.NameToLayer("Player");
    }

    void Update() {
        // �˴��e��O�_�����
        CheckWall();

        // �˴���νd�򤺪����a�ç���
        DetectAndAttackPlayer();
    }

    void FixedUpdate() {
        // ����
        Move();
    }

    void Move() {
        if (rb == null) {
            return;
        }

        if (isHorizontal) {
            rb.velocity = new Vector2(moveDirection * moveSpeed, 0);
        }
        else {
            rb.velocity = new Vector2(0, moveDirection * moveSpeed);
        }
    }

    void CheckWall() {
        // �ھڲ��ʶb�V�M�w�˴���V
        Vector2 direction;
        if (isHorizontal) {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // �g�u�˴��e��O�_����ê���]�������ɡ^
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, obstacleLayer);

        if (hit.collider != null) {
            // �I���ê���A��V
            Flip();
        }

        // ø�s�����g�u�]�Ȧb Scene ���Ϥ��i���^
        Debug.DrawRay(transform.position, direction * wallCheckDistance, Color.red);
    }

    void Flip() {
        // ���ಾ�ʤ�V
        moveDirection *= -1;

        // �p�G�O�������ʡA½��ϼh�]Sprite�^
        if (isHorizontal && spriteRenderer != null) {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // �p�G�O�������ʡA�i�H���½�� Y �b�]�i��^
        if (!isHorizontal && spriteRenderer != null) {
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }

    void DetectAndAttackPlayer() {
        // �ϥ� OverlapCircle �˴���νd�򤺪��Ҧ��I����
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, _attackTarget);

        var foundPlayer = false;

        if (hitColliders != null && hitColliders.Length > 0) {
            foreach (Collider2D hitCollider in hitColliders.Where(h => h.transform != transform)) {
                if (hitCollider != null) {
                    // �����쪱�a�A�i�����
                    if (hitCollider.transform.gameObject.layer == LayerMask.NameToLayer("Player")) foundPlayer = true;
                    Debug.Log("Enemy Patrol Circle �o�{���a�I");
                    Attack(hitCollider.gameObject);
                }
            }
        }

        // ��s������
        UpdateCircleDisplay();
    }

    void Attack(GameObject target) {
        if (target.TryGetComponent<IDamageable>(out var component)) {
            component.Damage();
        }
    }

    void SetupCircleRenderer() {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (lineRenderer == null) {
            Debug.LogError("�L�k�Ы� LineRenderer�I");

            return;
        }

        // �]�w LineRenderer �ݩ�
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth    = lineWidth;
        lineRenderer.endWidth      = lineWidth;
        lineRenderer.enabled       = false;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null) {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor       = circleColor;
        lineRenderer.endColor         = circleColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder     = 10;

        // ø�s���
        DrawCircle();

        if (!showDetectionCircle) {
            lineRenderer.enabled = false;
        }
    }

    void DrawCircle() {
        if (lineRenderer == null) return;

        float deltaTheta = (2f * Mathf.PI) / circleSegments;
        float theta      = 0f;

        for (int i = 0; i <= circleSegments; i++) {
            // �ϥΥ@�ɪŶ��y��
            float   x   = transform.position.x + detectionRadius * Mathf.Cos(theta);
            float   y   = transform.position.y + detectionRadius * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, y, transform.position.z);
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    void UpdateCircleDisplay() {
        if (!showDetectionCircle) {
            return;
        }

        // �C�V��s����m�]�]���ĤH�b���ʡ^
        DrawCircle();
    }

    // �b Scene ���Ϥ�ø�s�����d��
    void OnDrawGizmosSelected() {
        // �ھڲ��ʶb�V�M�w�˴���V�]�������˴��^
        Vector2 direction;
        if (isHorizontal) {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // ø�s����˴��d��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);

        // ø�s��ΰ����d��
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void Isolation() {
        _attackTarget |= 1 << LayerMask.NameToLayer("Enemy");
    }

    public void Damage() {
        Instantiate(_particle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}