using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyIdle : MonoBehaviour, IIsolationable, IDamageable
{
    [Header("�������@�]�w")]
    [SerializeField] private float detectionRadius = 3f; // ��ΰ����d��b�|

    [SerializeField] private float attackCooldown = 1f; // �����N�o�ɶ�
    private                  float lastAttackTime = 0f; // �W�������ɶ�

    [Header("���u��ܳ]�w")]
    [SerializeField] private bool showDetectionCircle = true; // �O�_��ܰ�����

    [SerializeField] private Color normalCircleColor = new Color(0, 1, 1, 0.8f); // ���`����C��
    [SerializeField] private Color attackCircleColor = new Color(1, 0, 0, 0.9f); // �����ɶ���C��
    [SerializeField] private int   circleSegments    = 50;                       // ����ӫ�
    [SerializeField] private float lineWidth         = 0.15f;                    // ���u���e��

    [SerializeField]
    ParticleSystem _particle;

    private Animator     animator;
    private LineRenderer lineRenderer;
    int                  _attackTarget;

    void Start() {
        // �]�w LineRenderer ø�s���
        SetupCircleRenderer();

        _attackTarget = 1 << LayerMask.NameToLayer("Player");
    }

    void Update() {
        // ������νd�򤺪����a�ç���
        DetectAndAttackPlayer();
    }

    void DetectAndAttackPlayer() {
        // �ϥ� OverlapCircle �˴���νd�򤺪��Ҧ��I����
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, _attackTarget);

        var validColliders =
            hitColliders
                .Where(h => h.transform != transform)
                .Where(h => {
                    return !(h.TryGetComponent<CharacterController>(out var component) &&
                             component.IsInvincible);
                })
                .ToList();

        if (validColliders.Any()) {
            foreach (Collider2D hitCollider in validColliders) {
                Debug.Log("Enemy Patrol Circle �o�{���a�I");
                Attack(hitCollider.gameObject);
            }

            Damage();
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
        lineRenderer.useWorldSpace = true; // ��Υ@�ɪŶ��A��ǽT
        lineRenderer.startWidth    = lineWidth;
        lineRenderer.endWidth      = lineWidth;
        lineRenderer.enabled       = false;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (lineMaterial != null) {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.startColor       = normalCircleColor;
        lineRenderer.endColor         = normalCircleColor;
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

        // �C�V��s����m�]�]���ĤH�i��|���ʡ^
        DrawCircle();

        // ��s�C��
        if (lineRenderer != null) {
            Color currentColor = normalCircleColor;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor   = currentColor;
        }
    }

    // �b Scene ���Ϥ�ø�s�����d��
    void OnDrawGizmosSelected() {
        // ø�s��ΰ����d�� - �o�����өM��ڰ����d�򧹥��@�P
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