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
            string emailLower = currentUser.Email.ToLower();
            DocumentReference docRef = db.Collection("users").Document(emailLower);

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
                            userEmailText.text = $"{nombre}";
                        }
                        catch
                        {
                            userEmailText.text = $"{currentUser.Email} (sin nombre)";
                            Debug.LogWarning("No se pudo obtener el campo 'name' correctamente.");
                        }
                    }
                    else
                    {
                        userEmailText.text = $"{currentUser.Email} (documento no existe)";
                        Debug.LogWarning("Documento del usuario no existe en Firestore.");
                    }
                }
                else
                {
                    userEmailText.text = $"{currentUser.Email} (error al cargar datos)";
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
