using UnityEngine;

// Attach this to a trigger or button at the end of each level
public class LevelToQuiz : MonoBehaviour
{
    public string levelName = "Level1"; // Set to "Level1", "Level2", or "Level3"
    public string quizSceneName = "QuizScene";
    
    // Call this when player reaches end of level
    public void TriggerQuiz()
    {
        QuizManager.LoadQuizForLevel(levelName, quizSceneName);
    }
    
    // Example: Trigger when player enters a collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerQuiz();
        }
    }
    
    // Or use OnTriggerEnter for 3D games
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerQuiz();
        }
    }
}