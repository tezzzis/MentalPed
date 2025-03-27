using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // Verificar si el usuario ya esta autenticado
        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            
            feedbackText.text = "Sesión iniciada: " + currentUser.Email;
            SceneManager.LoadScene("main");  
        }
        else
        {
           
            feedbackText.text = "No hay sesión iniciada.";
            SceneManager.LoadScene("login");
        }
    }

    public void Register()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        CreateUserWithEmailAndPassword(email, password);
    }

    private async void CreateUserWithEmailAndPassword(string email, string password)
    {
        try
        {
            var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = userCredential.User;
            feedbackText.text = "Usuario registrado: " + newUser.Email;
            SceneManager.LoadScene("login");
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "Error en el registro: " + ex.Message;
        }
    }

    public void Login()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        SignInWithEmailAndPassword(email, password);
    }

    private async void SignInWithEmailAndPassword(string email, string password)
    {
        try
        {
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = userCredential.User;
            feedbackText.text = "Inicio de sesión exitoso: " + user.Email;
            GoBack();
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "Error en el inicio de sesión: " + ex.Message;
        }
    }

    public void GoBack()
    {
       
        SceneManager.LoadScene("main");
    }

    public void Logout()
    {
        auth.SignOut();
        SceneManager.LoadScene("login");  
    }
}
