using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInventario : MonoBehaviour
{
    public GameObject VentanaInventario;
    public GameObject VentanaEscenario;

    public void CambioVentanaInv(){
        VentanaInventario.SetActive(true);
        VentanaEscenario.SetActive(false);
    }

    public void CambioVentanaEsc(){
        VentanaInventario.SetActive(false);
        VentanaEscenario.SetActive(true);
    }
}
