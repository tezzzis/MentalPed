using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eliminardatos22222 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void elimiardatos222(){
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Datos eliminados y guardados");
    }
}
