using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public struct LevelData
    {
        public string levelName;
        public int sceneIndex;
    }

    [Header("Data")]
    [SerializeField] private List<LevelData> levels;

    [Header("UI")]
    [SerializeField] private Transform container;
    [SerializeField] private TMP_Text levelTextPrefab;

    [Header("Input")]
    [SerializeField] XRInputValueReader<Vector2> LeftHandMoveInput = new XRInputValueReader<Vector2>("Left Hand Move");
    [SerializeField] private InputActionProperty selectInput;

    private List<TMP_Text> spawnedTexts = new List<TMP_Text>();
    private int currentIndex = 0;

    private float inputCooldown = 0.2f;
    private float inputTimer = 0f;

    private float timer = 0;

    void Start()
    {
        GenerateUI();
        UpdateSelection();
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 1.5f) HandleInput();
    }

    void GenerateUI()
    {
        foreach (var level in levels)
        {
            TMP_Text txt = Instantiate(levelTextPrefab, container);
            txt.text = level.levelName;
            spawnedTexts.Add(txt);
        }
    }

    void HandleInput()
    {
        inputTimer -= Time.deltaTime;

        float v = LeftHandMoveInput.ReadValue().y;

        if (inputTimer <= 0f)
        {
            if (v > 0.5f)
            {
                MoveSelection(-1);
            }
            else if (v < -0.5f)
            {
                MoveSelection(1);
            }
        }

        if (selectInput.action.ReadValue<float>() > 0.5)
        {
            SelectLevel();
        }
    }

    void MoveSelection(int dir)
    {
        currentIndex += dir;

        if (currentIndex < 0)
            currentIndex = levels.Count - 1;
        else if (currentIndex >= levels.Count)
            currentIndex = 0;

        inputTimer = inputCooldown;
        UpdateSelection();
    }

    void UpdateSelection()
    {
        for (int i = 0; i < spawnedTexts.Count; i++)
        {
            if (i == currentIndex)
            {
                spawnedTexts[i].fontStyle = FontStyles.Underline;
            }
            else
            {
                spawnedTexts[i].fontStyle = FontStyles.Normal;
            }
        }
    }

    void SelectLevel()
    {
        var level = levels[currentIndex];
        SceneManager.LoadScene(level.sceneIndex);
    }
}