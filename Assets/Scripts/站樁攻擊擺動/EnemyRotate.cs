using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotate : MonoBehaviour
{
    [Header("����]�w")]
    [SerializeField] private float rotationSpeed = 45f; // �C�����׼�
    [SerializeField] private bool clockwise = true; // true = ���ɰw, false = �f�ɰw
    [SerializeField] private float startAngle = 0f; // �_�l���ס]0 = �V�k�^
    [SerializeField] private float sweepAngle = 360f; // ���y���׽d��]360 = ������^

    [Header("�������@�]�w")]
    [SerializeField] private float visionDistance = 5f; // ���u�Z��
    [SerializeField] private float attackCooldown = 1f; // �����N�o�ɶ�
    private float lastAttackTime = 0f;

    [Header("���u��ܳ]�w")]
    [SerializeField] private bool showVisionLine = true;
    [SerializeField] private Color visionColor = Color.yellow; // ���u�C��
    [SerializeField] private float lineWidth = 0.05f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private float currentVisionAngle = 0f; // ���u��e����

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �}�l���� idle �� walk �ʵe
        if (animator != null)
        {
            animator.Play("Enemy idle"); // �� "Enemy walk"
        }

        // �]�w LineRenderer
        SetupLineRenderer();

        // ��l���u����
        currentVisionAngle = startAngle;
    }

    void Update()
    {
        // ������u���ס]�����ਤ�⥻���^
        RotateVision();

        // �˴����@�d�򤺪����a�ç���
        DetectAndAttackPlayer();
    }

    void RotateVision()
    {
        // �p�����
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        if (!clockwise)
        {
            rotationThisFrame = -rotationThisFrame;
        }

        currentVisionAngle += rotationThisFrame;

        // �p�G�]�w�F���y�d�򭭨�]���O 360 �ס^
        if (sweepAngle < 360f)
        {
            // �b�d�򤺨Ӧ^�\��
            float minAngle = startAngle;
            float maxAngle = startAngle + sweepAngle;

            if (currentVisionAngle > maxAngle || currentVisionAngle < minAngle)
            {
                // �����V
                clockwise = !clockwise;
                currentVisionAngle = Mathf.Clamp(currentVisionAngle, minAngle, maxAngle);
            }
        }
    }

    void DetectAndAttackPlayer()
    {
        // ���u��V�ھڷ�e���׭p��]���̿ફ�����^
        float angleInRadians = currentVisionAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // �Ыإ]�t��ê���M���a�� LayerMask
        LayerMask obstacleLayer = LayerMask.GetMask("obstacle");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        LayerMask combinedMask = obstacleLayer;

        if (playerLayerIndex >= 0)
        {
            combinedMask |= (1 << playerLayerIndex);
        }

        // �g�u�˴�
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionDistance, combinedMask);

        bool foundPlayer = false;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.Log("Enemy Rotate �o�{���a�I�����Z��: " + hit.distance);
            AttackPlayer(hit.collider.gameObject);
        }

        // ��s���u���
        UpdateVisionLine(direction, hit);

        // Debug ø�s
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
        lineRenderer.enabled = false;

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

        // �_�I
        lineRenderer.SetPosition(0, transform.position);

        // ���I
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
        // ø�s���@�]�ϥε��u���ס^
        float angleInRadians = currentVisionAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * visionDistance);

        // �p�G�����y�d�򭭨�Aø�s���y�d��
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