using UnityEngine;

/// Utility trigger that sends the player to the Quiz scene for a given level.
/// Attach to a collider trigger or call TriggerQuiz() from a UI button.
public class LevelToQuiz : MonoBehaviour
{
    [Header("Quiz Config")]
    [Tooltip("Must match the levelName configured in QuizManager.")]
    public string levelName      = "Level1";
    public string quizSceneName  = "Quiz";

    /// Loads the quiz scene for this level (can be called from UI events).
    public void TriggerQuiz() => QuizManager.LoadQuizForLevel(levelName, quizSceneName);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) TriggerQuiz();
    }
}
