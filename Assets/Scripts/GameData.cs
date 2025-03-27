using System;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
   

    public int coins;
    public int xp;
    public int beckTotalScore;

    // Misiones
    public List<string> completedMissions = new List<string>();
    public List<string> currentDailyMissions = new List<string>();

    // Preguntas
    public List<string> completedEmotionalQuestions = new List<string>();
    public List<string> completedBeckQuestions = new List<string>();
    public string currentEmotionalQuestion;
    public string currentBeckQuestion;

    // Tiempo

    public string lastLoginDate = "";
    public string lastResetDate = "";
    public string lastBeckResetDate = "";


    // Recompensas temporales
    public int pendingCoinReward;
    public int pendingXpReward;
}