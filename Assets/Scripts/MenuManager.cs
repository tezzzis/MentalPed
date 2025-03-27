using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void GoToMain()
    {
        SceneManager.LoadScene("main");
    }
    public void GoToGacha()
    {
        SceneManager.LoadScene("gacha");
    }
    public void GoToDiarias()
    {
        SceneManager.LoadScene("diaria");
    }
    public void GoToCasa()
    {
        SceneManager.LoadScene("casa");
    }
    public void GoToTienda()
    {
        SceneManager.LoadScene("tienda");
    }

    public void GoToRegistro()
    {
        SceneManager.LoadScene("registro");
    }
    public void GoToLogIn()
    {
        SceneManager.LoadScene("login");
    }
}
