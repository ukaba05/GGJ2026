using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [SerializeField]
    HorizontalLayoutGroup _layoutGroup;

    [SerializeField]
    Transform _arrow;

    int _selectedIndex;

    void Update() {
        var delta = -(int)Input.mouseScrollDelta.y;

        if (delta < 0 && _selectedIndex <= 0) return;

        if (delta > 0 && _selectedIndex >= _layoutGroup.transform.childCount - 1) return;
        _selectedIndex += delta;
        var slot = _layoutGroup.transform.GetChild(_selectedIndex);
        _arrow.SetParent(slot, false);
    }
}