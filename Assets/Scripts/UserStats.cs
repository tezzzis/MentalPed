using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserStats : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text xpText;
    void Start()
    {
        InitializeUserProfile();
    }
    private void InitializeUserProfile()
    {
        

        
        UpdateStatsDisplay();

        
        GameManager.Instance.OnDataChanged += UpdateStatsDisplay;
    }
    private void UpdateStatsDisplay()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameData == null)
        {
            Debug.LogError("GameManager no inicializado");
            return;
        }

        coinsText.text = GameManager.Instance.GameData.coins.ToString();
        xpText.text = GameManager.Instance.GameData.xp.ToString();
    }

    void OnDestroy()
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged -= UpdateStatsDisplay;
        }
    }
}
