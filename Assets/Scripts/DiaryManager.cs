using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using Firebase.Auth;
using TMPro;

public class DiaryManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button felizButton;
    public Button medioButton;
    public Button tristeButton;
    public TMP_InputField inputField;
    public Button guardarButton;

    private string emocionSeleccionada;
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        felizButton.onClick.AddListener(() => SeleccionarEmocion("feliz"));
        medioButton.onClick.AddListener(() => SeleccionarEmocion("medio"));
        tristeButton.onClick.AddListener(() => SeleccionarEmocion("triste"));
        guardarButton.onClick.AddListener(GuardarEntrada);

        inputField.gameObject.SetActive(false);
        guardarButton.gameObject.SetActive(false);
    }

    void SeleccionarEmocion(string emocion)
    {
        emocionSeleccionada = emocion;
        inputField.gameObject.SetActive(true);
        guardarButton.gameObject.SetActive(true);
    }

    async void GuardarEntrada()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            Debug.LogError("Usuario no autenticado");
            return;
        }

        if (string.IsNullOrEmpty(emocionSeleccionada) || string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogError("Faltan datos");
            return;
        }

        // Generar el ID con la fecha actual (formato: YYYY-MM-DD)
        string fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");

        // Referencia al documento con ID = fecha actual
        DocumentReference entryRef = db
            .Collection("users")
            .Document(user.Email)
            .Collection("diario")
            .Document(fechaHoy); // <- ID personalizado

        var entryData = new DiaryEntry
        {
            emocion = emocionSeleccionada,
            texto = inputField.text,
            fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        try
        {
            // Guardar o actualizar el documento
            await entryRef.SetAsync(entryData, SetOptions.MergeAll);
            Debug.Log("Entrada guardada con fecha: " + fechaHoy);

            // Limpiar UI
            inputField.text = "";
            inputField.gameObject.SetActive(false);
            guardarButton.gameObject.SetActive(false);
            emocionSeleccionada = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error al guardar: " + e.Message);
        }
    }
}

[FirestoreData]
public class DiaryEntry
{
    [FirestoreProperty]
    public string emocion { get; set; }

    [FirestoreProperty]
    public string texto { get; set; }

    [FirestoreProperty]
    public string fecha { get; set; }
}