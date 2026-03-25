using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TransitionToSceneOnInput : MonoBehaviour
{
    [SerializeField] private int sceneBuildIndex;
    [SerializeField] private InputActionProperty[] inputs;

    void Update()
    {
        foreach(InputActionProperty input in inputs)
        {
            if(input.action.WasPressedThisFrame())
            {
                ActivateScene();
            }
        }
    }

    private void ActivateScene()
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
