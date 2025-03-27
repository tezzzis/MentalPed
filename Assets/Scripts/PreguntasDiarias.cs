using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PreguntasDiarias
{
    public string questionID;
    public string questionText;
    public Sprite questionImage; 
    public List<Answer> answers = new List<Answer>(); 
}

[System.Serializable]
public class Answer
{
    public string answerText;
    public int coinReward;
    public int xpReward;
    public int score;
}

