using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class RespuestaManager : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
       
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firebase Firestore Initialized!");
        });
    }

    public void GuardarRespuesta(string userId, string preguntaId, string respuesta)
    {
        // Crear un objeto para guardar
        var respuestaData = new Dictionary<string, object>
        {
            { "userId", userId },
            { "preguntaId", preguntaId },
            { "respuesta", respuesta },
            { "timestamp", FieldValue.ServerTimestamp } 
        };

        db.Collection("respuestas").AddAsync(respuestaData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Respuesta guardada correctamente.");
            }
            else
            {
                Debug.LogError("Error al guardar respuesta: " + task.Exception);
            }
        });
    }
}
