using System.Threading.Tasks;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private FirebaseInitializer firebaseInitializer;

    void Start()
    {
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();

        if (firebaseInitializer != null)
        {
            // Esperar a que Firebase este inicializado
            firebaseInitializer.OnFirestoreInitialized += InitializeAuth;
        }
        else
        {
            Debug.LogError("FirebaseInitializer no encontrado");
        }
    }

    private async void InitializeAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        await VerifyCurrentUser();
    }

   private async Task VerifyCurrentUser()
{
    FirebaseUser currentUser = auth.CurrentUser;
    if (currentUser != null)
    {
        await currentUser.ReloadAsync(); // Recargar el usuario
        currentUser = auth.CurrentUser; // Obtener los datos actualizados

        if (!string.IsNullOrEmpty(currentUser.Email)) // Asegurar que los datos están completos
        {
            feedbackText.text = "Bienvenido de vuelta!";
            SceneManager.LoadScene("main");
        }
        else
        {
            feedbackText.text = "Error al cargar los datos del usuario";
        }
    }
}

    public void Login()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            SignInWithEmailAndPassword(email, password);
        }
        else
        {
            feedbackText.text = "Completa ambos campos";
        }
    }

    private async void SignInWithEmailAndPassword(string email, string password)
    {
        try
        {
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = userCredential.User;
            feedbackText.text = "Bienvenido: " + user.Email;
            await Task.Delay(2000);
            SceneManager.LoadScene("main");
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "Error: " + ex.Message;
        }
    }

    public void Logout()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            feedbackText.text = "Sesi�n cerrada";
            SceneManager.LoadScene("login");
        }
    }

    private string GetLoginErrorMessage(AuthError errorReason)
    {
        return errorReason switch
        {
            AuthError.UserNotFound => "Usuario no registrado",
            AuthError.WrongPassword => "Contraseña incorrecta",
            _ => "Error de autenticación: " + errorReason
        };
    }
}