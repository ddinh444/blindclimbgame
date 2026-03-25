using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToSceneOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private int sceneBuildIndex;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }

        SceneManager.LoadScene(sceneBuildIndex);
    }
}
