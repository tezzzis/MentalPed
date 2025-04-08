using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

public class cargarPersonaje : MonoBehaviour
{
    [Header("Referencias")]
    public Image cabezaImage;
    public Image cuerpoImage;
    public Image corbataImage;
    public Image zapatosImage;
    
    [Header("Sprites")]
    public List<Sprite> cabezas;
    public List<Sprite> cuerpos;
    public List<Sprite> corbata;
    public List<Sprite> zapatos;

    void Start()
    {
        CargarApariencia();
    }

    public void CargarApariencia()
    {
        // Cargar índices guardados (usando -1 como valor por defecto)
        int cabezaIndex = PlayerPrefs.GetInt("CabezaSeleccionada", -1);
        int cuerpoIndex = PlayerPrefs.GetInt("CuerpoSeleccionada", -1);
        int corbataIndex = PlayerPrefs.GetInt("CorbataSeleccionada", -1);
        int zapatosIndex = PlayerPrefs.GetInt("ZapatosSeleccionada", -1);
        
        // Aplicar cabeza si existe
        if (cabezaIndex >= 0 && cabezaIndex < cabezas.Count && cabezaImage != null)
        {
            cabezaImage.sprite = cabezas[cabezaIndex];
        }
        
        // Aplicar cuerpo si existe
        if (cuerpoIndex >= 0 && cuerpoIndex < cuerpos.Count && cuerpoImage != null)
        {
            cuerpoImage.sprite = cuerpos[cuerpoIndex];
        }

        if (corbataIndex >= 0 && corbataIndex < corbata.Count && corbataImage != null)
        {
            corbataImage.sprite = corbata[corbataIndex];
        }

        if (zapatosIndex >= 0 && zapatosIndex < zapatos.Count && zapatosImage != null)
        {
            zapatosImage.sprite = zapatos[zapatosIndex];
        }
    }
}