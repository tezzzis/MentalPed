using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Botones de Misiones")]
    public Button[] missionButtons; 
    public TMP_Text[] missionButtonTexts; 

    [Header("Preguntas Emocionales")]
    public Button emotionalQuestionButton;
    public TMP_Text emotionalQuestionButtonText;


    [Header("Panel Pregunta Emocional")]
    public GameObject emotionalQuestionPanel;
    public TMP_Text emotionalQuestionText;
    public Image questionImage;
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;
    public Button regresar;


    [Header("Preguntas Beck")]
    public Button beckQuestionButton;
    public TMP_Text beckQuestionButtonText;
   

    [Header("Panel Pregunta Beck")]
    public GameObject beckQuestionPanel;
    public TMP_Text beckQuestionText;
    public Image beckQuestionImage;
    public Button[] beckAnswerButtons;
    public TMP_Text[] beckAnswerTexts;
    public Button regresarbeck;

    [Header("Resultado Beck")]
    public GameObject beckResultPanel;
    public TMP_Text beckFinalScoreText;
    public Button regresarResultadobeck;


    void Start()
    {
       
        for (int i = 0; i < missionButtons.Length; i++)
        {
            int index = i; 
            missionButtons[i].onClick.AddListener(() => OnMissionButtonClicked(index));
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
        for (int i = 0; i < beckAnswerButtons.Length; i++)
        {
            int index = i;
            beckAnswerButtons[i].onClick.AddListener(() => OnBeckAnswerSelected(index));
        }
        emotionalQuestionButton.onClick.AddListener(OnEmotionalQuestionClicked);
        regresar.onClick.AddListener(Regresar);
        regresarbeck.onClick.AddListener(Regresarbeck);
        regresarResultadobeck.onClick.AddListener(Regresarbeck);
        beckQuestionButton.onClick.AddListener(OnBeckQuestionClicked);

        UpdateAllUI();
    }
    void SetupQuestionPanel(PreguntasDiarias question)
    {
        emotionalQuestionText.text = question.questionText;
        questionImage.sprite = question.questionImage;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Count)
            {
                answerTexts[i].text = question.answers[i].answerText;
                answerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }
    void Regresar()
    {
        emotionalQuestionPanel.SetActive(false);
    }
    void Regresarbeck()
    {
        beckQuestionPanel.SetActive(false);
        beckResultPanel.SetActive(false);

    }
    void OnAnswerSelected(int answerIndex)
    {

        var question = GameManager.Instance.GetCurrentEmotionalQuestion();
        var selectedAnswer = question.answers[answerIndex];
        if (GameManager.Instance.IsEmotionalQuestionAnswered()) return;
        if (question == null || answerIndex >= question.answers.Count) return;
        GameManager.Instance.AnswerEmotionalQuestion(selectedAnswer);
        
        GameManager.Instance.AddCoins(selectedAnswer.coinReward);
        GameManager.Instance.AddXP(selectedAnswer.xpReward);

        
        GameManager.Instance.CompleteEmotionalQuestion();

       UpdateAllUI();
        emotionalQuestionPanel.SetActive(false);
        
    }

    void OnMissionButtonClicked(int missionIndex)
    {
        
        List<Mission> dailyMissions = GameManager.Instance.GetCurrentDailyMissions();

        if (missionIndex < dailyMissions.Count)
        {
            Mission m = dailyMissions[missionIndex];
            GameManager.Instance.CompleteMission(m.missionID); 
            UpdateAllUI(); 
        }
    }

    void OnEmotionalQuestionClicked()
    {
        if (!GameManager.Instance.IsEmotionalQuestionAnswered())
        {
            var question = GameManager.Instance.GetCurrentEmotionalQuestion();
            if (question != null)
            {
                SetupQuestionPanel(question);
                emotionalQuestionPanel.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Ya respondiste esta pregunta");
        }
    }
    // ==================== BECK ====================
    void OnBeckQuestionClicked()
    {
        if (GameManager.Instance.AreAllBeckQuestionsAnswered())
        {
            Debug.Log("Todas las preguntas Beck respondidas");
            return;
        }

        if (string.IsNullOrEmpty(GameManager.Instance.GameData.currentBeckQuestion))
        {
            GameManager.Instance.GenerateBeckQuestion();
        }

        var question = GameManager.Instance.GetCurrentBeckQuestion();
        if (question != null)
        {
            SetupBeckPanel(question);
            beckQuestionPanel.SetActive(true);
        }
    }

    void SetupBeckPanel(BeckQuestion question)
    {
        beckQuestionText.text = question.questionText;
        beckQuestionImage.sprite = question.questionImage;

        for (int i = 0; i < beckAnswerButtons.Length; i++)
        {
            if (i < question.answers.Count)
            {
                beckAnswerTexts[i].text = question.answers[i].answerText;
                beckAnswerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                beckAnswerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnBeckAnswerSelected(int answerIndex)
    {
        if (GameManager.Instance.AreAllBeckQuestionsAnswered()) return;

        var question = GameManager.Instance.GetCurrentBeckQuestion();
        if (question == null || answerIndex >= question.answers.Count) return;

        var selectedAnswer = question.answers[answerIndex];
        GameManager.Instance.AnswerBeckQuestion(selectedAnswer);
        GameManager.Instance.AddCoins(selectedAnswer.coinReward);
        GameManager.Instance.AddXP(selectedAnswer.xpReward);
        GameManager.Instance.CompleteBeckQuestion();

        if (!GameManager.Instance.AreAllBeckQuestionsAnswered())
        {
            var nextQuestion = GameManager.Instance.GetCurrentBeckQuestion();
            if (nextQuestion != null)
            {
                SetupBeckPanel(nextQuestion);
            }
            else
            {
                beckQuestionPanel.SetActive(false);
            }
        }
        else
        {
            ShowBeckResults();
            beckQuestionPanel.SetActive(false);
        }

        UpdateAllUI();
    }
    void ShowBeckResults()
    {
        beckFinalScoreText.text = $"Puntaje total: {GameManager.Instance.GameData.beckTotalScore}/63";
        beckResultPanel.SetActive(true);

        // Interpretaci�n b�sica
        string interpretation = "";
        if (GameManager.Instance.GameData.beckTotalScore <= 13) interpretation = "Depresi�n m�nima";
        else if (GameManager.Instance.GameData.beckTotalScore <= 19) interpretation = "Depresi�n leve";
        else if (GameManager.Instance.GameData.beckTotalScore <= 28) interpretation = "Depresi�n moderada";
        else interpretation = "Depresi�n severa";

        beckFinalScoreText.text += $"\n({interpretation})";
    }



    public void UpdateAllUI()
    {
        UpdateMissionsUI();
        UpdateQuestionsUI();
    }

    void UpdateMissionsUI()
    {
        List<Mission> dailyMissions = GameManager.Instance.GetCurrentDailyMissions();

        for (int i = 0; i < missionButtons.Length; i++)
        {
            if (i < dailyMissions.Count)
            {
                Mission m = dailyMissions[i];
                missionButtonTexts[i].text = $"{m.description}\n          <sprite name=\"coin\"> {m.coinReward}";



               
                bool missionCompleted = GameManager.Instance.GameData.completedMissions.Contains(m.missionID);
                missionButtons[i].interactable = !missionCompleted; 
               
            }
            else
            {
                
                missionButtonTexts[i].text = "Misi�n no disponible";
                missionButtons[i].interactable = false;
                
            }
        }
    }

    void UpdateQuestionsUI()
    {
        bool isAnswered = GameManager.Instance.IsEmotionalQuestionAnswered();
        emotionalQuestionButton.interactable = !isAnswered;
        emotionalQuestionButtonText.text = isAnswered ?
            "" :
            "";


        bool allBeckAnswered = GameManager.Instance.AreAllBeckQuestionsAnswered();
        beckQuestionButton.interactable = !allBeckAnswered;
        beckQuestionButtonText.text = allBeckAnswered ? "" : "";

    }
}