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
    int[] _itemCount;

    int _selectedIndex;

    bool _enableMousedSelection;

    void Awake() {
        var instance = Instantiate(_slot, _layoutGroup.transform);
        _arrow.SetParent(instance);
        _arrow.localPosition = Vector2.up * 80;

        foreach (var count in _itemCount.Skip(1))
            Instantiate(_slot, _layoutGroup.transform);
    }

    void Update() {
        DetectMouseScroll();

        if (Input.GetKeyDown(KeyCode.X)) {
            TryUseItem(_selectedIndex);
        }

        if (_enableMousedSelection) { }
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
        Debug.Log(index);

        if (_itemCount[index] <= 0) return;
        _itemCount[index] -= 1;

        if (index == 0) {
            Investigation();
        }
    }

    void Investigation() {
        var worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        worldPoint = new Vector3(worldPoint.x, worldPoint.y, 0);
        var overlapPoint = Physics2D.OverlapPoint(worldPoint, LayerMask.GetMask("Enemy"));
        overlapPoint.GetComponent<LineRenderer>().enabled = true;
    }
}