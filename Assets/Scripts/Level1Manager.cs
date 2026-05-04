using UnityEngine;
using System.Collections.Generic;

/// Controls Level 1 (food sorting).
/// Spawns one random food item at a time from the list; when the list
/// is exhausted the player moves on to the Quiz.
public class Level1Manager : MonoBehaviour
{
    public static Level1Manager instance;

    [Header("Spawn")]
    public Transform        spawnPoint;

    [Header("Food Prefabs")]
    public List<GameObject> allFoods;   // serialised name kept intentionally

    [Header("Quiz Config")]
    public string levelID       = "Level1";
    public string quizSceneName = "Quiz";

    GameObject _currentFood;

    // ── Unity ─────────────────────────────────────────────────────────────

    void Awake() => instance = this;

    void Start() => SpawnNextFood();

    // ── Public API ────────────────────────────────────────────────────────

    /// Called by FoodDrag when a food item is correctly sorted.
    public void FoodDropped() => SpawnNextFood();

    // ── Helpers ───────────────────────────────────────────────────────────

    void SpawnNextFood()
    {
        if (allFoods == null || allFoods.Count == 0)
        {
            QuizManager.LoadQuizForLevel(levelID, quizSceneName);
            return;
        }

        int index    = Random.Range(0, allFoods.Count);
        _currentFood = Instantiate(allFoods[index], spawnPoint.position, Quaternion.identity);
        allFoods.RemoveAt(index);
    }
}
