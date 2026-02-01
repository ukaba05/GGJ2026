using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearLevel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneManager.LoadScene(4);
    }
}
