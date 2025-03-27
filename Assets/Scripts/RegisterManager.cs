using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using System.Collections.Generic;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    public TMP_Dropdown roleDropdown;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseInitializer firebaseInitializer;

    void Start()
    {
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();

        if (firebaseInitializer != null)
        {
           
            firebaseInitializer.OnFirestoreInitialized += InitializeAuth;
        }
        else
        {
            Debug.LogError("FirebaseInitializer no encontrado");
            feedbackText.text = "Error: Firebase no est� inicializado.";
        }
    }

    private void InitializeAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        Debug.Log("Firebase Auth inicializado correctamente.");
    }

    public void Register()
    {
        string name = nameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            CreateUserWithEmailAndPassword(email, password, name);
        }
        else
        {
            feedbackText.text = "Rellenar los campos es obligatorio";
        }
    }

    private async void CreateUserWithEmailAndPassword(string email, string password, string name)
    {
        if (auth == null)
        {
            feedbackText.text = "Error: Firebase Auth no est� inicializado.";
            return;
        }

        try
        {
            var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = userCredential.User;
            feedbackText.text = "Registro exitoso!";

            // 🔽 Guardar el valor del dropdown en Firestore
            string selectedRole = roleDropdown.options[roleDropdown.value].text;

            DocumentReference docRef = firestore.Collection("users").Document(email);
            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "email", email },
                { "name", name },
                { "role", selectedRole },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };
            await docRef.SetAsync(userData);

            auth.SignOut();
            await Task.Delay(2000);
            SceneManager.LoadScene("login");
        }
        catch (System.Exception ex)
        {
            FirebaseException firebaseEx = ex as FirebaseException;
            if (firebaseEx != null)
            {
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                feedbackText.text = GetRegisterErrorMessage(errorCode);
            }
            else
            {
                feedbackText.text = "Error desconocido: " + ex.Message;
            }
            Debug.LogError("Error en el registro: " + ex.ToString());
        }
    }

    private string GetRegisterErrorMessage(AuthError errorReason)
    {
        return errorReason switch
        {
            AuthError.EmailAlreadyInUse => "El correo ya est� registrado",
            AuthError.WeakPassword => "Contrase�a muy d�bil",
            _ => "Error de registro: " + errorReason
        };
    }
}