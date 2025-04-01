using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class Delog : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("Sesión cerrada con éxito.");
        }

        // Opcional: limpiar PlayerPrefs si se desea
        PlayerPrefs.DeleteAll();
        PlayerPrefs.DeleteKey("GameData");

        // Cargar escena de login
        SceneManager.LoadScene("login");
    }
}