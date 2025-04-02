using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class UserStats : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text xpText;
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        InitializeUserProfile();
    }

    private async void InitializeUserProfile()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No hay usuario autenticado.");
            return;
        }

        string userEmail = auth.CurrentUser.Email;

        DocumentReference userDoc = firestore.Collection("users").Document(userEmail);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue("coins", out int coins))
            {
                GameManager.Instance.GameData.coins = coins;
            }
            else
            {
                Debug.LogWarning("El campo 'coins' no existe en Firestore.");
            }

            UpdateStatsDisplay();
        }
        else
        {
            Debug.LogError("El documento del usuario no existe en Firestore.");
        }

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
        //xpText.text = GameManager.Instance.GameData.xp.ToString();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged -= UpdateStatsDisplay;
        }
    }
}
