using UnityEngine;
using System.Collections.Generic;

public class Level1Manager : MonoBehaviour
{
    public static Level1Manager instance;

    [Header("Where the food spawns")]
    public Transform spawnPoint;
    [Header("List of Foods (Drag the Prefabs here)")]

    [Header("Quiz Configuration")]
    public string levelID = "Level1";
    public string quizSceneName = "Quiz";
    public List<GameObject> allFoods; 

    private GameObject currentFood; // The food currently on the screen

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SpawnNextFood();
    }

    public void SpawnNextFood()
    {
        // 1. If there is no food left in the list...
        if (allFoods.Count == 0)
        {
            GoToQuiz();
        }

        // 2. Choose a random number from the list
        int randomIndex = Random.Range(0, allFoods.Count);

        // 3. Create the food at the spawn point
        currentFood = Instantiate(allFoods[randomIndex], spawnPoint.position, Quaternion.identity);

        // 4. Remove this food from the list to avoid repetition
        allFoods.RemoveAt(randomIndex);
    }

    // This function will be called by the food script when you get it right
    public void FoodCorrect()
    {
        SpawnNextFood();
    }

    void GoToQuiz()
    {
        QuizManager.LoadQuizForLevel(levelID, quizSceneName);
    }
}