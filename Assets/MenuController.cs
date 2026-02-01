using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    Button _startBtn;

    [SerializeField]
    Button _leaveButton;

    void Awake() {
        _startBtn.onClick.AddListener(() => {
            SceneManager.LoadScene(1);
        });

        _leaveButton.onClick.AddListener(Application.Quit);
    }
}