using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MaskUsesController : MonoBehaviour
{
    [SerializeField]
    Camera _camera;

    [SerializeField]
    HorizontalLayoutGroup _layoutGroup;

    [SerializeField]
    Transform _slot;

    [SerializeField]
    Transform _arrow;

    [SerializeField]
    ItemData[] _itemData;

    [SerializeField]
    GameObject _player;

    [SerializeField]
    float _invincibleDuration = 3f; // 無敵持續時間

    [SerializeField]
    float _blinkInterval = 0.1f; // 閃爍間隔

    [SerializeField]
    Color _invincibleColor = new Color(1f, 1f, 1f, 0.5f); // 無敵時的顏色

    int            _selectedIndex;
    bool           _isInvincible = false;
    SpriteRenderer _playerRenderer;
    Color          _originalColor;

    void Awake() {
        var instance = Instantiate(_slot, _layoutGroup.transform);
        instance.GetComponent<Image>().sprite = _itemData[0].sprite;
        _arrow.SetParent(instance);

        for (var i = 1; i < _itemData.Length; i++) {
            instance                              = Instantiate(_slot, _layoutGroup.transform);
            instance.GetComponent<Image>().sprite = _itemData[i].sprite;
        }

        _arrow.localPosition = Vector2.up * 80;
    }

    void Start() {
        // 在 Start 中初始化玩家相關的引用
        InitializePlayerRenderer();
    }


    void InitializePlayerRenderer() {
        // 如果沒有手動指定玩家，嘗試尋找
        if (_player == null) {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null) {
                Debug.LogWarning("找不到玩家物件！請在 Inspector 中指定 Player 或為玩家添加 'Player' Tag");

                return;
            }
        }

        // 獲取玩家的 SpriteRenderer
        _playerRenderer = _player.GetComponent<SpriteRenderer>();
        if (_playerRenderer != null) {
            _originalColor = _playerRenderer.color;
        }
        else {
            Debug.LogWarning("玩家物件上找不到 SpriteRenderer 組件！無敵閃爍效果將無法顯示");
        }
    }

    void Update() {
        DetectMouseScroll();
        if (Input.GetKeyDown(KeyCode.X)) {
            TryUseItem(_selectedIndex);
        }
    }

    void DetectMouseScroll() {
        var delta = -(int)Input.mouseScrollDelta.y;
        switch (delta) {
            case < 0 when _selectedIndex <= 0:
            case > 0 when _selectedIndex >= _layoutGroup.transform.childCount - 1:
                return;
        }

        _selectedIndex += delta;
        var slot = _layoutGroup.transform.GetChild(_selectedIndex);
        _arrow.SetParent(slot, false);
    }

    void TryUseItem(int index) {
        Debug.Log($"嘗試使用格子 {index} 的道具");

        // 檢查道具數量是否足夠
        if (_itemData[index].count <= 0) {
            Debug.Log($"格子 {index} 沒有道具了！");

            return;
        }

        _itemData[index].count -= 1;

        if (index == 0) {
            Investigation();

            return;
        }

        // 檢查是否是無敵道具格子
        if (index == 1) {
            // 使用無敵道具
            Debug.Log($"使用無敵道具！剩餘數量: {_itemData[index]}");
            ActivateInvincibility();

            return;
        }

        if (index == 2) {
            DoIsolation();
        }

        void DoIsolation() {
            var worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
            worldPoint = new Vector3(worldPoint.x, worldPoint.y, 0);
            var overlapPoint = Physics2D.OverlapPoint(worldPoint, LayerMask.GetMask("Enemy"));
            if (overlapPoint.TryGetComponent<IIsolationable>(out var component)) {
                component.Isolation();
            }
        }


        void Investigation() {
            var worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
            worldPoint = new Vector3(worldPoint.x, worldPoint.y, 0);
            var overlapPoint = Physics2D.OverlapPoint(worldPoint, LayerMask.GetMask("Enemy"));
            if (overlapPoint.TryGetComponent<LineRenderer>(out var component)) {
                component.enabled = true;
            }
        }
    }

    /// <summary>
    /// 啟動無敵狀態
    /// </summary>
    void ActivateInvincibility() {
        if (_isInvincible) {
            // 如果已經處於無敵狀態，重置計時器
            StopAllCoroutines();
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    /// <summary>
    /// 無敵狀態協程
    /// </summary>
    IEnumerator InvincibilityCoroutine() {
        _isInvincible = true;
        Debug.Log("⭐ 無敵狀態啟動！");

        float elapsed = 0f;

        // 閃爍效果
        while (elapsed < _invincibleDuration) {
            if (_playerRenderer != null) {
                _playerRenderer.color = _invincibleColor;
            }

            yield return new WaitForSeconds(_blinkInterval);

            if (_playerRenderer != null) {
                _playerRenderer.color = _originalColor;
            }

            yield return new WaitForSeconds(_blinkInterval);

            elapsed += _blinkInterval * 2;
        }

        // 恢復正常狀態
        if (_playerRenderer != null) {
            _playerRenderer.color = _originalColor;
        }

        _isInvincible = false;
        Debug.Log("⭐ 無敵狀態結束！");
    }

    [Serializable]
    public struct ItemData
    {
        public Sprite sprite;
        public int    count;
    }
}