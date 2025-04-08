using UnityEngine;
using TMPro;

public class MostrarCoins : MonoBehaviour
{
    public TMP_Text coinsText; // Asigna el componente TextMeshPro en el Inspector 

    void Start()
    {
        // Actualiza la visualización al iniciar
        UpdateCoinDisplay();
        
        // Suscribirse al evento OnDataChanged para actualizar cuando se modifiquen los datos
        if (GameManager.Instance != null)
{
    GameManager.Instance.OnDataChanged += UpdateCoinDisplay;
}
    }

    // Método para actualizar la UI con el valor de monedas
    void UpdateCoinDisplay()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            coinsText.text = GameManager.Instance.GameData.coins.ToString();
        }
        else
        {
            coinsText.text = "0";
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento al destruirse el objeto para evitar referencias nulas
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged -= UpdateCoinDisplay;
        }
    }
}