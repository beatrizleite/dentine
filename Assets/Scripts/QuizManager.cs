using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuizOption
    {
        public enum OptionType { Text, Image, Both }
        
        public OptionType type = OptionType.Text;
        public string text;
        public Sprite image;
    }

    [System.Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public AudioClip questionAudio;
        [Header("Always 2 Options")]
        public QuizOption option1;
        public QuizOption option2;
        public int correctAnswerIndex; // 0 = option1, 1 = option2
    }

    [System.Serializable]
    public class LevelQuizData
    {
        public string levelName; // "Level1", "Level2", "Level3"
        public QuizQuestion[] questions;
        public string returnScene;
        public string nextScene;
    }

    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button audioButton;
    public Button backButton;
    public GameObject option1Button;
    public GameObject option2Button;
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;
    public Image[] starImages;
    public Button continueButton;
    
    [Header("All Level Quiz Data")]
    public LevelQuizData[] levelQuizzes;
    
    [Header("Star Sprites")]
    public UnityEngine.UI.Image[] starImages5;   // always 5 slots
    public Sprite starFilled;
    public Sprite starEmpty;

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

    public static string currentLevel = "Level1";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioButton.onClick.AddListener(PlayQuestionAudio);
        continueButton.onClick.AddListener(ContinueToNextLevel);
        
        resultsPanel.SetActive(false);
        
        LoadLevelQuiz(currentLevel);
        LoadQuestion(currentQuestionIndex);
    }

    void LoadLevelQuiz(string levelName)
    {
        foreach (LevelQuizData levelData in levelQuizzes)
        {
            if (levelData.levelName == levelName)
            {
                currentLevelData = levelData;
                        return;
            }
        }
        
        if (levelQuizzes.Length > 0)
        {
            currentLevelData = levelQuizzes[0];
        }
        else
        {
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

        // Setup option 1
        SetupOptionButton(option1Button, question.option1);
        ResetButtonColor(option1Button);
        Button btn1 = option1Button.GetComponent<Button>();
        btn1.onClick.RemoveAllListeners();
        btn1.onClick.AddListener(() => OnOptionSelected(0));

        // Setup option 2
        SetupOptionButton(option2Button, question.option2);
        ResetButtonColor(option2Button);
        Button btn2 = option2Button.GetComponent<Button>();
        btn2.onClick.RemoveAllListeners();
        btn2.onClick.AddListener(() => OnOptionSelected(1));
        
        isProcessingAnswer = false;
    }

    void SetupOptionButton(GameObject button, QuizOption option)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = button.transform.Find("OptionImage")?.GetComponent<Image>();

        switch (option.type)
        {
            case QuizOption.OptionType.Text:
                if (buttonText != null)
                {
                    buttonText.gameObject.SetActive(true);
                    buttonText.text = option.text;
                }
                if (buttonImage != null)
                {
                    buttonImage.gameObject.SetActive(false);
                }
                break;

            case QuizOption.OptionType.Image:
                if (buttonText != null)
                {
                    buttonText.gameObject.SetActive(false);
                }
                if (buttonImage != null)
                {
                    buttonImage.gameObject.SetActive(true);
                    buttonImage.sprite = option.image;
                    buttonImage.preserveAspect = true;
                }
                break;

            case QuizOption.OptionType.Both:
                if (buttonText != null)
                {
                    buttonText.gameObject.SetActive(true);
                    buttonText.text = option.text;
                }
                if (buttonImage != null)
                {
                    buttonImage.gameObject.SetActive(true);
                    buttonImage.sprite = option.image;
                    buttonImage.preserveAspect = true;
                }
                break;
        }
    }

    void ResetButtonColor(GameObject button)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = Color.white;
        }
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
            GameObject wrongButton = selectedIndex == 0 ? option1Button : option2Button;
            StartCoroutine(ShowWrongAnswerFeedback(wrongButton));
        }
    }

    IEnumerator ShowWrongAnswerFeedback(GameObject wrongButton)
    {
        Image buttonImage = wrongButton.GetComponent<Image>();
        buttonImage.color = wrongAnswerColor;
        
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
        resultsText.text = $"Respostas Corretas: {correctAnswers}\nRespostas Erradas: {wrongAnswers}";
        
        int stars = CalculateStars(correctAnswers, totalQuestions);
        SaveStars(stars);

        // Use starImages5 (always 5 visible) with filled/empty sprites
        var displayArr = (starImages5 != null && starImages5.Length > 0) ? starImages5 : starImages;
        for (int i = 0; i < displayArr.Length; i++)
        {
            if (displayArr[i] == null) continue;
            displayArr[i].enabled = true;
            if (starFilled != null && starEmpty != null)
                displayArr[i].sprite = i < stars ? starFilled : starEmpty;
            else
                displayArr[i].enabled = i < stars;
        }
    }

    int CalculateStars(int correct, int total)
    {
        // 1 star per correct answer (max = total questions)
        return Mathf.Clamp(correct, 0, starImages.Length);
    }

    void SaveStars(int stars)
    {
        string saveKey = currentLevelData.levelName + "_Stars";
        int previousStars = PlayerPrefs.GetInt(saveKey, 0);
        
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(saveKey, stars);
            PlayerPrefs.Save();
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
    
    public static void LoadQuizForLevel(string levelName, string quizSceneName = "Quiz")
    {
        currentLevel = levelName;
        SceneManager.LoadScene(quizSceneName);
    }
}