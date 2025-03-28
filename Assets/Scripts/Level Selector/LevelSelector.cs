using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class LevelSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform gridParent;
    private int levelCount = 1;

    private void Start()
    {
        LoadLevelButtons();
    }

    void LoadLevelButtons()
    {
        string[] levels = GetLevelsFromFolder();

        foreach (string levelName in levels)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, gridParent);
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => LoadLevel(levelName));

            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = levelCount.ToString();
            levelCount++;
        }
    }

    string[] GetLevelsFromFolder()
    {
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");
        string[] levelNames = new string[levelFiles.Length];

        for (int i = 0; i < levelFiles.Length; i++)
        {
            levelNames[i] = levelFiles[i].name;
        }

        return levelNames;
    }

    void LoadLevel(string levelName)
    {
        // Set the path in GameManager before loading the scene
        GameManager.SelectedLevelName = levelName; // Store the name only, not the full path
        Debug.Log("Selected Level: " + levelName);
        SceneManager.LoadScene("GameLevel");
    }
}