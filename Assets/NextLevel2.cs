using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel2 : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        SceneManager.LoadScene(3);
    }
}
