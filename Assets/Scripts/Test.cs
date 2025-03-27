using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeckQuestion
{
    public string questionID;
    public string questionText;
    public Sprite questionImage; 
    public List<Answer> answers = new List<Answer>(); 
}

