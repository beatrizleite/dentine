using UnityEngine;
using System.Collections.Generic;

public class Level1Manager : MonoBehaviour
{
    public static Level1Manager instance;

    public Transform spawnPoint;
    public List<GameObject> allFoods; // name kept for scene serialisation
    public string levelID       = "Level1";
    public string quizSceneName = "Quiz";

    void Awake() => instance = this;
    void Start()  => SpawnNextFood();

    public void FoodDropped() => SpawnNextFood();

    void SpawnNextFood()
    {
        if (allFoods == null || allFoods.Count == 0)
        {
            QuizManager.LoadQuizForLevel(levelID, quizSceneName);
            return;
        }

        int i = Random.Range(0, allFoods.Count);
        Instantiate(allFoods[i], spawnPoint.position, Quaternion.identity);
        allFoods.RemoveAt(i);
    }
}
