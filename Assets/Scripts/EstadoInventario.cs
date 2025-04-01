using UnityEngine;
using UnityEngine.UI;

public class CargarEstadoInventario : MonoBehaviour
{
    void Start()
    {
        // Comprobar si existen los datos guardados del color
        if (PlayerPrefs.HasKey("ColorR") && PlayerPrefs.HasKey("ColorG") && PlayerPrefs.HasKey("ColorB"))
        {
            float r = PlayerPrefs.GetFloat("ColorR");
            float g = PlayerPrefs.GetFloat("ColorG");
            float b = PlayerPrefs.GetFloat("ColorB");
            Color savedColor = new Color(r, g, b);

            // Asignar el color al componente Image del objeto actual
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.color = savedColor;
                Debug.Log("Color cargado y aplicado: " + savedColor);
            }
            else
            {
                Debug.LogError("No se encontró el componente Image en el objeto.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron datos guardados para el color.");
        }
    }
}