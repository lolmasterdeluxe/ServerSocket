using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnPool : MonoBehaviour
{
    public GameObject columnPrefab;
    public int columnPoolSize = 5;
    public float spawnRate = 1f;
    public float columnMin = -1f;
    public float columnMax = 3.5f;

    private GameObject[] columns;
    private int currentColumn = 0;

    private Vector2 objectPoolPosition = new Vector2(-15, -25);
    private float spawnXPosition = 20.0f;

    private float timeSinceLastSpawned;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceLastSpawned = spawnRate / 2.0f;

        //Initialze the columns collection
        columns = new GameObject[columnPoolSize];
        //Loop and create the columns and store to the array
        for(int i = 0; i < columnPoolSize; ++i)
        {
            columns[i] = (GameObject)Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity);
        }
        currentColumn = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpawned += Time.deltaTime;

        if (GameController.instance.isGameOver == false && timeSinceLastSpawned >= spawnRate)
        {
            //Reset the timer 
            timeSinceLastSpawned = 0.0f;

            //Set a random Y position for the column
            float spawnYPosition = Random.Range(columnMin, columnMax);

            //Move the current column to that position
            columns[currentColumn].transform.position = new Vector2(spawnXPosition, spawnYPosition);

            int difficultyLevel = Random.Range(10, 20) - 10;
            columns[currentColumn].GetComponent<Column>().Setup(difficultyLevel);

            //Increament the current column
            currentColumn++;

            //Reset the current column if exceed
            if(currentColumn >= columnPoolSize)
            {
                currentColumn = 0;
            }
        }
        
    }
}
