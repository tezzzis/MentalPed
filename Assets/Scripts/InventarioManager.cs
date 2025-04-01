using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class InventarioManager : MonoBehaviour
{
    public Transform contenedorItems; // El parent donde están todos los items
    public GameObject panelCompra;      // Panel de compra asignado desde el Inspector
    public Text textoPopup;             // Texto dentro del panel de compra
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
        
        if (pruebaTransform != null)
        {
            Image img = pruebaTransform.GetComponent<Image>();
            if (img != null)
            {
                img.color = desbloqueado ? Color.white : Color.gray;
                Debug.Log($"Color cambiado para {item.name}");
            }
            else
            {
                Debug.LogError($"No se encontró componente Image en prueba de {item.name}");
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
            Debug.LogError($"No se encontró el objeto 'prueba' en {item.name}");
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

    // Devuelve un color basado en el nombre del item seleccionado
    Color ObtenerColorParaItem(string nombreItem)
    {
        Dictionary<string, Color> colores = new Dictionary<string, Color>
        {
            { "prueba_Escenario1", Color.red },
            { "prueba_Escenario2", Color.blue },
            { "prueba_Escenario3", Color.green },
            { "prueba_Escenario4", Color.cyan },
            { "prueba_Escenario5", Color.magenta }
        };

        if (colores.ContainsKey(nombreItem))
        {
            return colores[nombreItem];
        }
        
        Debug.LogWarning($"El nombre '{nombreItem}' no está en el diccionario, asignando color amarillo.");
        return Color.yellow;
    }

    void MostrarPanelCompra(string claveItem, GameObject itemObjeto)
    {
        if (panelCompra != null)
        {
            panelCompra.SetActive(true);
            
            if (textoPopup != null)
            {
                textoPopup.text = $"¿Deseas comprar {claveItem}?";
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
        int monedasDisponibles = 100; // Aquí deberías obtener las monedas reales del usuario
        int precio = 50;

        if (monedasDisponibles >= precio)
        {
            monedasDisponibles -= precio;

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
