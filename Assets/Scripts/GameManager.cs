using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    
    public List<Mission> allMissions = new List<Mission>();
    public List<PreguntasDiarias> allEmotionalQuestions = new List<PreguntasDiarias>();
    public List<BeckQuestion> allBeckQuestions = new List<BeckQuestion>();
    [SerializeField] private GameData gameData1;
    [SerializeField] public TimeSpan resetHour = new TimeSpan(9, 18, 0);

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userEmail;
    private FirebaseInitializer firebaseInitializer;
    public event Action OnDataChanged;


    public GameData GameData
    {
        get { return gameData; }
    }
    public PreguntasDiarias GetCurrentEmotionalQuestion()
    {
        return allEmotionalQuestions.Find(q => q.questionID == gameData.currentEmotionalQuestion);
    }
    public BeckQuestion GetCurrentBeckQuestion()
    {
        return allBeckQuestions.Find(q => q.questionID == gameData.currentBeckQuestion);
    }

    public List<Mission> GetCurrentDailyMissions()
    {
        return gameData.currentDailyMissions
            .Select(id => allMissions.Find(m => m.missionID == id))
            .ToList();
    }

    private GameData gameData;

    void Awake()
    {
        if (Instance == null)
        {
            
            Instance = this;
            DebugPlayerPrefs();
            DontDestroyOnLoad(gameObject);
           // Debug_ResetDaily();
            InitializeGameData();
            // Buscar el inicializador de Firebase
            firebaseInitializer = FindObjectOfType<FirebaseInitializer>();
            if (firebaseInitializer != null)
            {
                firebaseInitializer.OnFirebaseReady += InitializeFirebaseComponents;
            }
           

        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    private void InitializeFirebaseComponents()
    {
        db = firebaseInitializer.GetFirestore();
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        HandleAuthChange(auth.CurrentUser);

        Debug.Log("Componentes de Firebase inicializados en GameManager");
    }
    private void AuthStateChanged(object sender, EventArgs e)
    {
        HandleAuthChange(auth.CurrentUser);
    }
    private void HandleAuthChange(FirebaseUser user)
    {
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            userEmail = user.Email;
            Debug.Log("Usuario autenticado: " + userEmail);
        }
        else
        {
            Debug.Log("Usuario no autenticado.");
        }
    }


    void InitializeGameData()
    {
        LoadGameData();

        DateTime nowLocal = DateTime.Now;
        DateTime todayReset = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 1, 0, 0);
        DateTime lastReset = gameData.lastResetDate != "" ?
        DateTime.Parse(gameData.lastResetDate) :
        DateTime.MinValue;

        Debug.Log("HORA ACTUAL (LOCAL): " + nowLocal.ToString("dd/MM/yyyy HH:mm:ss"));
        Debug.Log("HORA RESET (LOCAL): " + todayReset.ToString("dd/MM/yyyy HH:mm:ss"));
        Debug.Log("HORA GUARDADA (LOCAL): " + lastReset.ToString("dd/MM/yyyy HH:mm:ss"));

        // Verificar si la hora actual es mayor a la hora del reset
        if (nowLocal >= todayReset)
        {
            //ResetBeckContent();
            // ResetDailyContent();
            if (lastReset < todayReset)
            {
                ResetDailyContent();
                gameData.lastResetDate = DateTime.Now.ToString("o");
                SaveGameData();
                Debug.Log("RESET APLICADO");
            }
            else
            {
                Debug.Log("NO SE RESETEA: Ya se ha reseteado hoy.");
            }
        }
        else
        {
            Debug.Log("NO SE RESETEA: La hora actual es antes de las 5 AM.");
        }

        DateTime lastBeckReset = gameData.lastBeckResetDate != "" ?
            DateTime.Parse(gameData.lastBeckResetDate) :
            DateTime.MinValue;

        if ((nowLocal - lastBeckReset).TotalDays >= 15)
        {
            ResetBeckContent();
            gameData.lastBeckResetDate = nowLocal.ToString("o");
            Debug.Log("Reset de Beck aplicado (15 d�as)");
        }
        
    }

    void OnApplicationQuit()
    {
        UpdateLastLoginTime();
    }

    void UpdateLastLoginTime()
    {
        gameData.lastLoginDate = DateTime.Now.ToString("o"); // Formato ISO 8601
        Debug.Log("hora ultimo login" + gameData.lastLoginDate);
        SaveGameData();
    }

  

    public void ResetDailyContent()
    {

        Debug.Log("Reset diario ejecutado");
        GenerateDailyMissions();
        GenerateEmotionalQuestion();
        
        CheckFullBankReset();
    }
    void ResetBeckContent()
    {
        Debug.Log("Contenido de Beck reseteado");
        CheckFullBankReset();
        gameData.currentBeckQuestion = "";
        gameData.beckTotalScore = 0;
        GenerateBeckQuestion();
        
    }

    void GenerateDailyMissions()
    {
        gameData.currentDailyMissions.Clear();

        var available = allMissions
            .Where(m => !gameData.completedMissions.Contains(m.missionID))
            .ToList();

        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            gameData.currentDailyMissions.Add(available[randomIndex].missionID);
            available.RemoveAt(randomIndex);
        }
    }
    public void CompleteEmotionalQuestion()
    {
        if (!gameData.completedEmotionalQuestions.Contains(gameData.currentEmotionalQuestion))
        {
            gameData.completedEmotionalQuestions.Add(gameData.currentEmotionalQuestion);
        }
        gameData.currentEmotionalQuestion = "";
        SaveGameData();
    }
    public bool IsEmotionalQuestionAnswered()
    {
        return gameData.completedEmotionalQuestions.Contains(gameData.currentEmotionalQuestion);
    }
    public void AddCoins(int amount)
    {
        gameData.coins += amount;
        OnDataChanged?.Invoke();
        Debug.Log("asdsadsaddsa------------------------------------------------------------------------");


        SaveGameData();
        UpdateCoinsInFirestore(gameData.coins);
    }

    public void AddXP(int amount)
    {
        gameData.xp += amount;
        OnDataChanged?.Invoke();
        SaveGameData();
    }


    void GenerateEmotionalQuestion()
    {
        // Obtener todas las preguntas emocionales disponibles que no hayan sido completadas
        var available = allEmotionalQuestions
            .Where(q => !gameData.completedEmotionalQuestions.Contains(q.questionID))
            .ToList();

        // Verificar si hay preguntas disponibles
        if (available.Count > 0)
        {
            // Si hay preguntas disponibles, seleccionar una aleatoriamente
            var selected = available[UnityEngine.Random.Range(0, available.Count)];
            gameData.currentEmotionalQuestion = selected.questionID;
        }
        else
        {
            Debug.LogWarning("No hay preguntas emocionales disponibles");
        }
    }

    public bool AreAllBeckQuestionsAnswered()
    {
        return gameData.completedBeckQuestions.Count >= allBeckQuestions.Count;
    }

    public void CompleteBeckQuestion()
    {
        if (!string.IsNullOrEmpty(gameData.currentBeckQuestion) &&
            !gameData.completedBeckQuestions.Contains(gameData.currentBeckQuestion))
        {
            gameData.completedBeckQuestions.Add(gameData.currentBeckQuestion);
        }

        // Generar nueva pregunta solo si no se han completado todas
        if (gameData.completedBeckQuestions.Count < allBeckQuestions.Count)
        {
            GenerateBeckQuestion();
        }
        else
        {
            gameData.currentBeckQuestion = "";
        }

        SaveGameData();
        OnDataChanged?.Invoke();
    }

    public void GenerateBeckQuestion()
    {
        var available = allBeckQuestions
            .Where(q => !gameData.completedBeckQuestions.Contains(q.questionID))
            .ToList();

        if (available.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            gameData.currentBeckQuestion = available[randomIndex].questionID;
        }
        else
        {
            gameData.currentBeckQuestion = "";
        }

        SaveGameData();
    }

    public void CompleteMission(string missionID)
    {
        var mission = allMissions.Find(m => m.missionID == missionID);

        gameData.coins += mission.coinReward;
        gameData.xp += mission.xpReward;
        gameData.completedMissions.Add(missionID);
        gameData.currentDailyMissions.Remove(missionID);

        Debug.Log("asdsadsaddsa------------------------------------------------------------------------");


        SaveGameData();
        UpdateCoinsInFirestore(gameData.coins);
    }

    public void AnswerEmotionalQuestion(Answer selectedAnswer)
    {
        if (!gameData.completedEmotionalQuestions.Contains(gameData.currentEmotionalQuestion))
        {
            // Subir respuesta a Firebase
            UploadQuestionResponse(
                gameData.currentEmotionalQuestion,
                selectedAnswer.answerText,
                "emotionalQuestions",
                selectedAnswer.score
                
            );

           
        }
    }

    public void AnswerBeckQuestion(Answer selectedAnswer)
    {
        if (!gameData.completedBeckQuestions.Contains(gameData.currentBeckQuestion))
        {
            // Sumar puntaje
            gameData.beckTotalScore += selectedAnswer.score;

            UploadQuestionResponse(
                gameData.currentBeckQuestion,
                selectedAnswer.answerText,
                "beckQuestions",
                selectedAnswer.score // <- A�adir par�metro
            );
        }
    }

    private void UploadQuestionResponse(string questionId, string answer, string questionType, int score)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Usuario no autenticado");
            return;
        }

        var responseData = new
        {
            questionId = questionId,
            answer = answer,
            score = score, // <- Nuevo campo
            type = questionType,
            timestamp = DateTime.UtcNow.ToString("dd-MM")
        };


        var responsesRef = db.Collection("users")
                            .Document(userEmail)
                            .Collection("gameResponses");

        responsesRef.AddAsync(responseData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error subiendo respuesta: " + task.Exception);
            }
            else
            {
                Debug.Log("Respuesta subida exitosamente a Firebase");
            }
        });

      
        var questionRef = db.Collection("users")
                          .Document(userEmail)
                          .Collection("completedQuestions")
                          .Document(questionId);

        var updateData = new
        {
            completed = true,
            lastCompletion = DateTime.UtcNow
        };

        questionRef.SetAsync(updateData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error actualizando estado: " + task.Exception);
            }
        });
    }

    void CheckFullBankReset()
    {
        if (gameData.completedMissions.Count >= allMissions.Count)
            gameData.completedMissions.Clear();

        if (gameData.completedEmotionalQuestions.Count >= allEmotionalQuestions.Count)
            gameData.completedEmotionalQuestions.Clear();

        if (gameData.completedBeckQuestions.Count >= allBeckQuestions.Count)
            gameData.completedBeckQuestions.Clear();
    }

    void ClearPendingRewards() => gameData.pendingCoinReward = gameData.pendingXpReward = 0;

 

    public void SaveGameData()
    {
       
        string json = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();
    }

    
    void LoadGameData()
    {
        if (PlayerPrefs.HasKey("GameData"))
        {
            string json = PlayerPrefs.GetString("GameData");
            gameData = JsonUtility.FromJson<GameData>(json);
            if (gameData.lastLoginDate == null) gameData.lastLoginDate = "";
            if (gameData.lastResetDate == null) gameData.lastResetDate = "";
        }
        else
        {
            gameData = new GameData();
        }
    }


    void DebugPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("GameData"))
        {
            string json = PlayerPrefs.GetString("GameData");
            Debug.Log("Saved GameData: " + json);

        }
        else
        {
            Debug.Log("No GameData found in PlayerPrefs.");
        }
    }

    public void Delog()
    {
        if (auth != null)
        {
            auth.SignOut();
            PlayerPrefs.DeleteKey("GameData");
            SceneManager.LoadScene("login");
        }else{
            Debug.Log("no funciona estaasdasdsadsad");
        }
        
    }

    public void SpendCoins(int amount)
{
    if (gameData.coins >= amount)
    {
        gameData.coins -= amount;
        OnDataChanged?.Invoke();
        SaveGameData();
        UpdateCoinsInFirestore(gameData.coins);
        Debug.Log("Monedas gastadas: " + amount + ", monedas restantes: " + gameData.coins);
    }
    else
    {
        Debug.Log("No hay suficientes monedas para gastar");
    }
}

/// <summary>
/// Actualiza la cantidad de monedas en Firebase Firestore.
/// </summary>
private void UpdateCoinsInFirestore(int newCoinAmount)
{
    Debug.Log("asdsadsaddsa------------------------------------------------------------------------");

    if (string.IsNullOrEmpty(userEmail))
    {
        Debug.LogError("No se puede actualizar Firestore: Usuario no autenticado.");
        return;
    }

    DocumentReference userRef = db.Collection("users").Document(userEmail);
    userRef.UpdateAsync(new Dictionary<string, object>
    {
        { "coins", newCoinAmount }
    }).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("Monedas actualizadas en Firestore: " + newCoinAmount);
        }
        else
        {
            Debug.LogError("Error actualizando monedas en Firestore: " + task.Exception);
        }
    });
}


}