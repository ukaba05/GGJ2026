using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class MaskUsesController : MonoBehaviour
{
    [SerializeField]
    HorizontalLayoutGroup _layoutGroup;

    [SerializeField]
    Transform _slot;

    [SerializeField]
    Transform _arrow;

    [SerializeField]
    int[] _itemCount;

    int _selectedIndex;

    void Awake() {
        var instance = Instantiate(_slot, _layoutGroup.transform);
        _arrow.SetParent(instance);
        _arrow.localPosition = Vector2.up * 80;

        foreach (var count in _itemCount.Skip(1))
            Instantiate(_slot, _layoutGroup.transform);
    }

    void Update() {
        MouseScroll();
        if (Input.GetKeyDown(KeyCode.X)) {
            UseItem();
        }
    }

    void MouseScroll() {
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

    void UseItem() {
        Debug.Log(_selectedIndex);
        _itemCount[_selectedIndex] -= 1;
    }
}