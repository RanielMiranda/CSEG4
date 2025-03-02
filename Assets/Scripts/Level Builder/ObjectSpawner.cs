using UnityEngine;
using TMPro;

public class ObjectSpawner : MonoBehaviour
{
    public TMP_Dropdown objectDropdown;
    public Transform spawnPoint;

    public void SpawnSelectedObject()
    {
        string selected = objectDropdown.options[objectDropdown.value].text;
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + selected);

        // Check if the object is a player
        if (prefab != null && prefab.name == "Player")
        {
            // Check the amount of players
            int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
            if (playerCount >= 1)
            {
                Debug.Log("You already have a player in the scene.");
                return;
            }
        }
        // Check if the object is a Goal
        else if (prefab != null && prefab.name == "Goal")
        {
            // Check the amount of goals
            int goalCount = GameObject.FindGameObjectsWithTag("Goal").Length;
            if (goalCount >= 1)
            {
                Debug.Log("You already have a goal in the scene.");
                return;
            }
        }

        // Set the y position based on the prefab
        float y;
        if (prefab != null)
        {
            if (prefab.name == "Player")
            {
                y = .25f;
            }
            else if (prefab.name == "Wall" || prefab.name == "Goal")
            {
                y = .5f;
            }
            else if (prefab.name == "Pressure Plate")
            {
                y = 0.05f;
            }
            else
            {
                y = 0.5f;
            }

            spawnPoint.position = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);  
            GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);  
                                            
        }
        else
        {
            Debug.LogError("Prefab not found: " + selected);
        }
    }
}

