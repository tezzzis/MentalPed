using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;


public class InventarioManager : MonoBehaviour
{
    public Transform contenedorItems; // El parent donde están todos los items
    public GameObject panelCompra;      // Panel de compra asignado desde el Inspector
    public TextMeshProUGUI textoPopup;         // Texto dentro del panel de compra
    public Button btnComprar, btnCancelar; // Botones del popup
    public GameObject objetoSeleccionado;  // Objeto que cambiará de color si está desbloqueado

    private FirebaseFirestore db;
    private FirebaseUser user;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        
        // Ocultar el panel de compra al iniciar
        if (panelCompra != null) 
        {
            panelCompra.SetActive(false);
            Debug.Log("Panel de compra inicializado correctamente");
        }
        else
        {
            Debug.LogError("Panel de compra no asignado en el Inspector");
        }
        
        // Cargar el color guardado para el objetoSeleccionado (si existe)
        CargarColorGuardado();

        if (user != null)
        {
            CargarEstadoInventario();
        }
        else
        {
            Debug.LogWarning("Usuario no autenticado");
            // Para pruebas, configura todos los items como bloqueados
            ConfigurarItemsSinConexion();
        }
    }

    // Carga el color previamente guardado usando PlayerPrefs
    void CargarColorGuardado()
    {
        if (PlayerPrefs.HasKey("ColorR") && PlayerPrefs.HasKey("ColorG") && PlayerPrefs.HasKey("ColorB"))
        {
            float r = PlayerPrefs.GetFloat("ColorR");
            float g = PlayerPrefs.GetFloat("ColorG");
            float b = PlayerPrefs.GetFloat("ColorB");
            Color savedColor = new Color(r, g, b);
            if (objetoSeleccionado != null)
            {
                Image img = objetoSeleccionado.GetComponent<Image>();
                if (img != null)
                {
                    img.color = savedColor;
                    Debug.Log("Color cargado: " + savedColor);
                }
            }
        }
    }

    // Método para pruebas sin conexión (sin Firebase)
    void ConfigurarItemsSinConexion()
    {
        foreach (Transform item in contenedorItems)
        {
            string nombreItem = item.name;
            string claveBD = ObtenerClaveDesdeNombre(nombreItem);
            if (!string.IsNullOrEmpty(claveBD))
            {
                ConfigurarItem(item.gameObject, false, claveBD);
            }
        }
    }

    async void CargarEstadoInventario()
    {
        try
        {
            DocumentReference docRef = db.Collection("users").Document(user.Email);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Debug.Log("Datos obtenidos correctamente");
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey("escenarios"))
                {
                    Dictionary<string, object> escenarios = (Dictionary<string, object>)data["escenarios"];

                    foreach (Transform item in contenedorItems)
                    {
                        string nombreItem = item.name; // ejemplo: prueba_Esce1
                        string claveBD = ObtenerClaveDesdeNombre(nombreItem); // ejemplo: escenario_1

                        if (!string.IsNullOrEmpty(claveBD) && escenarios.ContainsKey(claveBD))
                        {
                            bool desbloqueado = (bool)escenarios[claveBD];
                            ConfigurarItem(item.gameObject, desbloqueado, claveBD);
                        }
                        else
                        {
                            // Si no existe en la BD, configurarlo como bloqueado
                            ConfigurarItem(item.gameObject, false, claveBD);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No se encontró la clave 'escenarios' en la BD");
                    ConfigurarItemsSinConexion();
                }
            }
            else
            {
                Debug.LogWarning("No existe el documento en la BD");
                ConfigurarItemsSinConexion();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar datos: " + e.Message);
            ConfigurarItemsSinConexion();
        }
    }

    string ObtenerClaveDesdeNombre(string nombre)
    {
        if (nombre.StartsWith("prueba_Esce"))
        {
            string num = nombre.Replace("prueba_Escenario", "");
            return $"escenario_{num}";
        }
        return "";
    }

    void ConfigurarItem(GameObject item, bool desbloqueado, string claveBD)
{
    Debug.Log($"Configurando item: {item.name}, desbloqueado: {desbloqueado}");
    
    // Buscar el objeto "prueba" dentro del item
    Transform pruebaTransform = item.transform.Find("prueba");
    Transform textoTransform = item.transform.Find("Text (TMP)"); // Buscar el texto

    if (pruebaTransform != null && textoTransform != null)
    {
        Image img = pruebaTransform.GetComponent<Image>();
        TMPro.TextMeshProUGUI textoTMP = textoTransform.GetComponent<TMPro.TextMeshProUGUI>();

        if (img != null && textoTMP != null)
        {
            if (desbloqueado)
            {
                Color colorAsignado = ObtenerColorParaItem(item.name);
                img.color = Color.white; // Se puede dejar en blanco para indicar desbloqueado
                
                // Obtener el nombre del color desde el diccionario
                string nombreColor = nombresColores.ContainsKey(item.name) ? nombresColores[item.name] : "Color Desconocido";
                textoTMP.text = nombreColor; // Se muestra el nombre del color
            }
            else
            {
                img.color = Color.gray; // Indicar que está bloqueado
                textoTMP.text = "$50"; // Precio del objeto
            }

            Debug.Log($"Texto actualizado para {item.name}: {textoTMP.text}");
        }
        else
        {
            Debug.LogError($"No se encontró componente Image o TextMeshPro en {item.name}");
        }

        // Configurar el botón en el item
        Button btn = item.GetComponent<Button>();
        if (btn == null)
        {
            btn = item.AddComponent<Button>();
            Debug.Log($"Botón agregado a {item.name}");
        }

        btn.onClick.RemoveAllListeners();

        if (desbloqueado)
        {
            btn.onClick.AddListener(() => {
                Debug.Log($"Item desbloqueado seleccionado: {item.name}");
                SeleccionarItem(item);
            });
        }
        else
        {
            btn.onClick.AddListener(() => {
                Debug.Log($"Mostrando panel de compra para: {item.name}");
                MostrarPanelCompra(claveBD, item);
            });
        }
    }
    else
    {
        Debug.LogError($"No se encontró el objeto 'prueba' o 'Text (TMP)' en {item.name}");
    }
}

    // Al seleccionar un item se cambia el color del objetoSeleccionado y se guarda
    void SeleccionarItem(GameObject item)
    {
        if (objetoSeleccionado != null)
        {
            Image imgObjeto = objetoSeleccionado.GetComponent<Image>();
            if (imgObjeto != null)
            {
                string nombreItem = item.name;
                Color nuevoColor = ObtenerColorParaItem(nombreItem);

                Debug.Log($"Item seleccionado: {nombreItem}, Color asignado: {nuevoColor}");
                imgObjeto.color = nuevoColor;

                // Guardar el color de manera persistente
                GuardarColor(nuevoColor);
            }
            else
            {
                Debug.LogError("El objetoSeleccionado no tiene un componente Image");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado objetoSeleccionado en el Inspector");
        }
    }

    // Guarda el color usando PlayerPrefs
    void GuardarColor(Color color)
    {
        PlayerPrefs.SetFloat("ColorR", color.r);
        PlayerPrefs.SetFloat("ColorG", color.g);
        PlayerPrefs.SetFloat("ColorB", color.b);
        PlayerPrefs.Save();
        Debug.Log("Color guardado: " + color);
    }

    /// guardar nombree
    Dictionary<string, string> nombresColores = new Dictionary<string, string>
{
    { "prueba_Escenario1", "Turquesa Claro" },
    { "prueba_Escenario2", "Gris Azulado" },
    { "prueba_Escenario3", "Verde Pastel" },
    { "prueba_Escenario4", "Durazno" },
    { "prueba_Escenario5", "Lavanda" },
    { "prueba_Escenario6", "Aqua Suave" },
    { "prueba_Escenario7", "Gris Neutro" },
    { "prueba_Escenario8", "Lila Claro" }
};

    // Devuelve un color basado en el nombre del item seleccionado
    Color ObtenerColorParaItem(string nombreItem)
    {
        Dictionary<string, Color> colores = new Dictionary<string, Color>
        {
            { "prueba_Escenario1", HexToColor("#E0F2F1") },
            { "prueba_Escenario2", HexToColor("#CDD5E0") },
            { "prueba_Escenario3", HexToColor("#E8EFCF") },
            { "prueba_Escenario4", HexToColor("#EDC6B1") },
            { "prueba_Escenario5", HexToColor("#D1B2DF") },
            { "prueba_Escenario6", HexToColor("#A1CCD1") },
            { "prueba_Escenario7", HexToColor("#B7B7B7") },
            { "prueba_Escenario8", HexToColor("#F2E1FF") }
        };

        if (colores.ContainsKey(nombreItem))
        {
            return colores[nombreItem];
        }

        
        
        Debug.LogWarning($"El nombre '{nombreItem}' no está en el diccionario, asignando color amarillo.");
        return Color.yellow;
    }

    Color HexToColor(string hex)
{
    Color color;
    if (ColorUtility.TryParseHtmlString(hex, out color))
    {
        return color;
    }
    return Color.white; // Color por defecto si el formato es inválido
}

     void MostrarPanelCompra(string claveItem, GameObject itemObjeto)
    {
        if (panelCompra != null)
        {
            panelCompra.SetActive(true);

            string nombreColor = nombresColores.ContainsKey(itemObjeto.name) ? nombresColores[itemObjeto.name] : "Color Desconocido";

            if (textoPopup != null)
            {
                textoPopup.text = $"¿Quieres comprar {nombreColor} por $50?";
            }
            else
            {
                Debug.LogError("textoPopup no está asignado en el Inspector");
            }

            if (btnComprar != null && btnCancelar != null)
            {
                btnComprar.onClick.RemoveAllListeners();
                btnCancelar.onClick.RemoveAllListeners();

                btnComprar.onClick.AddListener(() => RealizarCompra(claveItem, itemObjeto));
                btnCancelar.onClick.AddListener(() => panelCompra.SetActive(false));
            }
            else
            {
                Debug.LogError("btnComprar o btnCancelar no están asignados en el Inspector");
            }
        }
        else
        {
            Debug.LogError("panelCompra no está asignado en el Inspector");
        }
    }


    async void RealizarCompra(string claveItem, GameObject itemObjeto)
{
    int precio = 50;
    
    // Verificar que se tienen suficientes monedas usando GameManager
    if (GameManager.Instance.GameData.coins >= precio)
    {
        // Gasta las monedas mediante el método del GameManager
        GameManager.Instance.SpendCoins(precio);

        try
        {
            DocumentReference docRef = db.Collection("users").Document(user.Email);
            await docRef.UpdateAsync($"escenarios.{claveItem}", true);
            
            Debug.Log($"Base de datos actualizada para {claveItem}");
            ConfigurarItem(itemObjeto, true, claveItem);
            panelCompra.SetActive(false);
            Debug.Log("Compra realizada con éxito");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al actualizar la BD: " + e.Message);
        }
    }
    else
    {
        Debug.Log("No tienes suficientes monedas");
        // Aquí puedes mostrar otro popup indicando que no hay suficientes monedas
    }
}
}
