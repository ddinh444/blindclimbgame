using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TransitionToSceneOnInput : MonoBehaviour
{
    [SerializeField] private int sceneBuildIndex;
    [SerializeField] private InputActionProperty input;

    void OnEnable()
    {
        input.action.Enable();
    }

    void OnDisable()
    {
        input.action.Disable();
    }

    void Update()
    {
        if (input.action.triggered)
        {
            ActivateScene();
        }
    }

    private void ActivateScene()
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
