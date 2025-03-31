using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using Firebase.Extensions;

public class UserProfileDisplay : MonoBehaviour
{
    public TMP_Text userEmailText;

    private FirebaseInitializer firebaseInitializer;

    void Start()
    {
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();

        if (firebaseInitializer != null)
        {
            firebaseInitializer.OnFirestoreInitialized += InitializeUserProfile;
        }
        else
        {
            Debug.LogError("FirebaseInitializer no encontrado");
        }
    }

    private void InitializeUserProfile()
    {
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser != null)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            // Usamos el correo como ID del documento
            DocumentReference docRef = db.Collection("users").Document(currentUser.Email);

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    DocumentSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        try
                        {
                            string nombre = snapshot.GetValue<string>("name");
                            userEmailText.text = $"Usuario: {nombre}";
                        }
                        catch
                        {
                            userEmailText.text = $"Usuario: {currentUser.Email} (sin nombre)";
                            Debug.LogWarning("No se pudo obtener el campo 'name' correctamente.");
                        }
                    }
                    else
                    {
                        userEmailText.text = $"Usuario: {currentUser.Email} (documento no existe)";
                        Debug.LogWarning("Documento del usuario no existe en Firestore.");
                    }
                }
                else
                {
                    userEmailText.text = $"Usuario: {currentUser.Email} (error al cargar datos)";
                    Debug.LogError("Error al obtener documento desde Firestore: " + task.Exception);
                }
            });
        }
        else
        {
            userEmailText.text = "No autenticado";
        }
    }

    void OnDestroy()
    {
        if (firebaseInitializer != null)
        {
            firebaseInitializer.OnFirestoreInitialized -= InitializeUserProfile;
        }
    }
}
