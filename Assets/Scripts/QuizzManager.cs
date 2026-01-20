using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public AudioClip questionAudio;
        public string[] options; // 2-4 options
        public int correctAnswerIndex;
    }

    [System.Serializable]
    public class LevelQuizData
    {
        public string levelName; // "Level1", "Level2", "Level3"
        public QuizQuestion[] questions;
        public string returnScene; // Scene to return to when back is pressed
        public string nextScene; // Scene to load after quiz completion
    }

    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button audioButton;
    public Button backButton;
    public GameObject[] optionButtons; // Array to hold 2-4 option buttons
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;
    public Image[] starImages; // 3 star images
    public Button continueButton;
    
    [Header("All Level Quiz Data")]
    public LevelQuizData[] levelQuizzes; // Array of 3 level quizzes
    
    [Header("Visual Feedback")]
    public Color wrongAnswerColor = Color.red;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 10f;
    public float wrongAnswerDelay = 1f;
    
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private AudioSource audioSource;
    private bool isProcessingAnswer = false;
    private LevelQuizData currentLevelData;

    // Static variable to pass level info between scenes
    public static string currentLevel = "Level1";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioButton.onClick.AddListener(PlayQuestionAudio);
        backButton.onClick.AddListener(GoBack);
        continueButton.onClick.AddListener(ContinueToNextLevel);
        
        resultsPanel.SetActive(false);
        
        // Load the appropriate quiz based on currentLevel
        LoadLevelQuiz(currentLevel);
        LoadQuestion(currentQuestionIndex);
    }

    void LoadLevelQuiz(string levelName)
    {
        // Find the matching level quiz data
        foreach (LevelQuizData levelData in levelQuizzes)
        {
            if (levelData.levelName == levelName)
            {
                currentLevelData = levelData;
                Debug.Log($"Loaded quiz for {levelName}");
                return;
            }
        }
        
        // Fallback to first level if not found
        if (levelQuizzes.Length > 0)
        {
            currentLevelData = levelQuizzes[0];
            Debug.LogWarning($"Level {levelName} not found, using {currentLevelData.levelName}");
        }
        else
        {
            Debug.LogError("No quiz data configured!");
        }
    }

    void LoadQuestion(int index)
    {
        if (currentLevelData == null || currentLevelData.questions == null) return;
        
        if (index >= currentLevelData.questions.Length)
        {
            ShowResults();
            return;
        }

        QuizQuestion question = currentLevelData.questions[index];
        questionText.text = question.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < question.options.Length)
            {
                optionButtons[i].SetActive(true);
                TextMeshProUGUI buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = question.options[i];
                
                // Reset button color
                Image buttonImage = optionButtons[i].GetComponent<Image>();
                buttonImage.color = Color.white;
                
                Button btn = optionButtons[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int optionIndex = i;
                btn.onClick.AddListener(() => OnOptionSelected(optionIndex));
            }
            else
            {
                optionButtons[i].SetActive(false);
            }
        }
        
        isProcessingAnswer = false;
    }

    void PlayQuestionAudio()
    {
        if (currentLevelData.questions[currentQuestionIndex].questionAudio != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            audioSource.clip = currentLevelData.questions[currentQuestionIndex].questionAudio;
            audioSource.Play();
        }
    }

    void OnOptionSelected(int selectedIndex)
    {
        if (isProcessingAnswer) return;
        isProcessingAnswer = true;
        
        QuizQuestion question = currentLevelData.questions[currentQuestionIndex];
        
        if (selectedIndex == question.correctAnswerIndex)
        {
            correctAnswers++;
            currentQuestionIndex++;
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            wrongAnswers++;
            StartCoroutine(ShowWrongAnswerFeedback(selectedIndex));
        }
    }

    IEnumerator ShowWrongAnswerFeedback(int wrongButtonIndex)
    {
        GameObject wrongButton = optionButtons[wrongButtonIndex];
        Image buttonImage = wrongButton.GetComponent<Image>();
        buttonImage.color = wrongAnswerColor;
        
        // Shake animation
        Vector3 originalPos = wrongButton.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-shakeIntensity, shakeIntensity);
            float y = originalPos.y + Random.Range(-shakeIntensity, shakeIntensity);
            wrongButton.transform.localPosition = new Vector3(x, y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        wrongButton.transform.localPosition = originalPos;
        
        yield return new WaitForSeconds(wrongAnswerDelay);
        
        currentQuestionIndex++;
        LoadQuestion(currentQuestionIndex);
    }

    void ShowResults()
    {
        resultsPanel.SetActive(true);
        
        int totalQuestions = currentLevelData.questions.Length;
        resultsText.text = $"Correct: {correctAnswers}\nWrong: {wrongAnswers}";
        
        // Calculate stars (3 stars = all correct, 2 stars = 70%+, 1 star = 40%+)
        int stars = CalculateStars(correctAnswers, totalQuestions);
        
        // Save stars with level-specific key
        SaveStars(stars);
        
        // Display stars
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].enabled = i < stars;
        }
    }

    int CalculateStars(int correct, int total)
    {
        float percentage = (float)correct / total;
        
        if (percentage >= 1f) return 3;
        if (percentage >= 0.7f) return 2;
        if (percentage >= 0.4f) return 1;
        return 0;
    }

    void SaveStars(int stars)
    {
        string saveKey = currentLevelData.levelName + "_Stars";
        int previousStars = PlayerPrefs.GetInt(saveKey, 0);
        
        // Only save if new score is better
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(saveKey, stars);
            PlayerPrefs.Save();
            Debug.Log($"Saved {stars} stars for {currentLevelData.levelName}");
        }
    }

    public static int GetSavedStars(string levelName)
    {
        return PlayerPrefs.GetInt(levelName + "_Stars", 0);
    }

    void ContinueToNextLevel()
    {
        SceneManager.LoadScene(currentLevelData.nextScene);
    }

    void GoBack()
    {
        SceneManager.LoadScene(currentLevelData.returnScene);
    }
    
    // Call this from your level scenes before loading the quiz
    public static void LoadQuizForLevel(string levelName, string quizSceneName = "QuizScene")
    {
        currentLevel = levelName;
        SceneManager.LoadScene(quizSceneName);
    }
}