using System.Linq;
using UnityEngine;

public class EnemyWalk : MonoBehaviour, IISolationable
{
    [Header("���ʳ]�w")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private bool isHorizontal  = true; // true = x�b����, false = y�b����
    private                  int  moveDirection = 1;    // 1 �� -1

    [Header("�����]�w")]
    [SerializeField] private float wallCheckDistance = 0.5f;

    private LayerMask obstacleLayer; // ��ê���ϼh

    [Header("�������@�]�w")]
    [SerializeField] private float visionDistance = 5f;

    [SerializeField] private float attackCooldown = 1f; // �����N�o�ɶ�
    private                  float lastAttackTime = 0f; // �W�������ɶ�

    [Header("���u��ܳ]�w")]
    [SerializeField] private bool showVisionLine = true; // �O�_��ܵ��u

    [SerializeField] private Color normalVisionColor = Color.yellow; // ���`���u�C��
    [SerializeField] private float lineWidth         = 0.05f;        // �u���e��

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

        // �]�w LineRenderer
        SetupLineRenderer();

        _attackTarget =  1 << LayerMask.NameToLayer("Player");
    }

    void Update() {
        // �˴��e��O�_�����
        CheckWall();

        // �˴����@�d�򤺪����a�ç���
        DetectAndAttackPlayer();
    }

    void FixedUpdate() {
        // ����
        Move();
    }

    void Move() {
        if (rb == null) return;

        if (isHorizontal) {
            // x �b����
            rb.velocity = new Vector2(moveDirection * moveSpeed, 0);
        }
        else {
            // y �b����
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
        // �ھڲ��ʶb�V�M�w���u��V
        Vector2 direction;
        if (isHorizontal) {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // �g�u�˴����@�d��A����I���ê���Ϊ��a
        var origin = transform.position + (Vector3)direction ;
        var hit =
            Physics2D
                .Raycast(origin, direction, visionDistance, _attackTarget);


        bool foundPlayer = false;

        if (hit.collider != null) {
            // ���u�������a�A�i�����
            Debug.Log("Enemy Walk " + hit.transform.gameObject);
            AttackPlayer(hit.collider.gameObject);
            foundPlayer = true;
        }

        // ��s���u���
        UpdateVisionLine(direction, hit, foundPlayer);

        // ø�s���@�d��]�Ȧb Scene ���Ϥ��i���^
        float drawDistance = hit.collider != null ? hit.distance : visionDistance;
        Debug.DrawRay(origin, direction * drawDistance, foundPlayer ? Color.red : Color.yellow);
    }

    void AttackPlayer(GameObject player) {
        // �ˬd�����N�o�ɶ�
        if (Time.time - lastAttackTime < attackCooldown) {
            return; // �٦b�N�o���A�������
        }

        // ��s�W�������ɶ�
        lastAttackTime = Time.time;
    }

    // �b Scene ���Ϥ�ø�s���@�d��
    void OnDrawGizmosSelected() {
        Vector2 direction;
        if (isHorizontal) {
            direction = moveDirection > 0 ? Vector2.right : Vector2.left;
        }
        else {
            direction = moveDirection > 0 ? Vector2.up : Vector2.down;
        }

        // ø�s���@
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // ø�s����˴��d��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);
    }

    void SetupLineRenderer() {
        // �ˬd�O�_�w�� LineRenderer�A�S���h�K�[
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // �]�w LineRenderer �ݩ�
        lineRenderer.startWidth       = lineWidth;
        lineRenderer.endWidth         = lineWidth;
        lineRenderer.positionCount    = 2;
        lineRenderer.material         = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor       = normalVisionColor;
        lineRenderer.endColor         = normalVisionColor;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder     = 10; // �T�O�b��L����W�����
        lineRenderer.enabled          = false;

        // �p�G����ܵ��u�A�h�T��
        if (!showVisionLine) {
            lineRenderer.enabled = false;
        }
    }

    void UpdateVisionLine(Vector2 direction, RaycastHit2D hit, bool foundPlayer) {
        if (lineRenderer == null || !showVisionLine) {
            return;
        }

        // �]�w�_�I
        lineRenderer.SetPosition(0, transform.position);

        // �]�w���I�]�p�G�I��F��A�N�e��I���I�F�_�h�e��̻��Z���^
        Vector3 endPoint;
        if (hit.collider != null) {
            endPoint = hit.point;
        }
        else {
            endPoint = (Vector2)transform.position + direction * visionDistance;
        }

        lineRenderer.SetPosition(1, endPoint);
    }

    public void Isolation() {
        _attackTarget |= 1 << LayerMask.NameToLayer("Enemy");
    }
}