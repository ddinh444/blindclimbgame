using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private int deathSceneIndex;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }

        SceneManager.LoadScene(deathSceneIndex);
    }
}
