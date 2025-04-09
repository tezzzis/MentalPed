using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;

public class InventarioPersonajeManager : MonoBehaviour
{
     [Header("Contenedores de Items")]
    public Transform contenedorCabeza;
    public Transform contenedorCuerpo;
    public Transform contenedorZapatos;
    public Transform contenedorCorbata;

    [Header("UI")]
    public GameObject panelCompra;
    public TextMeshProUGUI textoPopup;
    public Button btnComprar, btnCancelar;
    public Image imagenCabezaSeleccionada;
    public Image imagenCuerpoSeleccionada;
    public Image imagenZapatosSeleccionada;
    public Image imagenCorbataSeleccionada;

    [Header("Configuración de Imágenes")]
    public List<Sprite> imagenesCabeza;
    public List<Sprite> imagenesCuerpo;
    public List<Sprite> imagenesZapatos;
    public List<Sprite> imagenesCorbata;
    public Material grayscaleMaterial;

    // Diccionario de nombres personalizados
    private Dictionary<string, string> nombresItems = new Dictionary<string, string>()
    {
        // Nombres para items de cabeza
        {"cabeza_1", "Nada"},
        {"cabeza_2", "Gorro"},
        { "cabeza_3", "Moño" },
        { "cabeza_4", "Boina" },
        { "cabeza_5", "Gorra" },
        { "cabeza_6", "Sombrero vaquero" },
        { "cabeza_7", "Sombrero elegante" },
        { "cabeza_8", "Sombrero fiestero" },
        { "cabeza_9", "Gorro de cumpleaños" },
        { "cabeza_10", "Bonbín" },
        { "cabeza_11", "Gorro de juglar" },
        { "cabeza_12", "Tajada de pan" },
        { "cabeza_13", "Pera" },
        { "cabeza_14", "Calabaza" },

        
        // Nombres para items de cuerpo
        {"cuerpo_1", "Nada"},
        {"cuerpo_2", "Gafas de corazones"},
        { "cuerpo_3", "Gafas negras" },
        { "cuerpo_4", "Gafas de estrellas" },
        { "cuerpo_5", "Gafas rosas" },
        { "cuerpo_6", "Gafas naranjas" },
        { "cuerpo_7", "Gafas blancas" },
        { "cuerpo_8", "Gafas retro" },
        { "cuerpo_9", "Gafas 3D" },
        { "cuerpo_10", "Gafas cool" },
        { "cuerpo_11", "Nariz de payaso" },

        { "corbata_1", "Nada"},
        { "corbata_2", "Corbatin negro"},
        { "corbata_3", "corbatin turquesa"},
        { "corbata_4", "Pañoleta"},
        { "corbata_5", "Collar"},
        { "corbata_6", "Pañoleta roja"},
        { "corbata_7", "Bufanda"},
        { "corbata_8", "Collar de perlas"},

        { "zapatos_1", "Nada" },
        { "zapatos_2", "Zapatos rojos" },
        { "zapatos_3", "Tacones" },
        { "zapatos_4", "Botas" },
        { "zapatos_5", "Zapatos amarillos" },
        { "zapatos_6", "Zapatos negros" },
        { "zapatos_7", "Zapatos payaso" },
        { "zapatos_8", "Zapatos cafes" }
    };

    private Dictionary<string, int> preciosItems = new Dictionary<string, int>()
{
    { "cabeza_1", 0 }, // Nada siempre puede ser gratis
    { "cabeza_2", 50 },
    { "cabeza_3", 70 },
    { "cabeza_4", 90 },
    { "cabeza_5", 90 },
    { "cabeza_6", 90 },
    { "cabeza_7", 90 },
    { "cabeza_8", 90 },
    { "cabeza_9", 90 },
    { "cabeza_10", 90 },
    { "cabeza_11", 90 },
    { "cabeza_12", 90 },
    { "cabeza_13", 90 },
    { "cabeza_14", 90 },

    { "cuerpo_1", 0 },
    { "cuerpo_2", 60 },
    { "cuerpo_3", 60 },
    { "cuerpo_4", 75 },
    { "cuerpo_5", 80 },
    { "cuerpo_6", 100 },
    { "cuerpo_7", 100 },
    { "cuerpo_8", 100 },
    { "cuerpo_9", 100 },
    { "cuerpo_10", 100 },
    { "cuerpo_11", 100 },

    { "corbata_1", 0 },
    { "corbata_2", 50 },
    { "corbata_3", 55 },
    { "corbata_4", 65 },
    { "corbata_5", 85 },
    { "corbata_6", 85 },
    { "corbata_7", 85 },
    { "corbata_8", 85 },

    { "zapatos_1", 0 },
    { "zapatos_2", 60 },
    { "zapatos_3", 70 },
    { "zapatos_4", 80 },
    { "zapatos_5", 80 },
    { "zapatos_6", 80 },
    { "zapatos_7", 80 },
    { "zapatos_8", 90 }
};

    private FirebaseFirestore db;
    private FirebaseUser user;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        
        if (panelCompra != null) panelCompra.SetActive(false);
        
        CargarSeleccionesGuardadas();

        if (user != null)
        {
            CargarEstadoInventario();
        }
        else
        {
            ConfigurarItemsSinConexion();
        }
    }

    void CargarSeleccionesGuardadas()
    {
        int cabezaIndex = PlayerPrefs.GetInt("CabezaSeleccionada", -1);
        int cuerpoIndex = PlayerPrefs.GetInt("CuerpoSeleccionada", -1);
        int zapatosIndex = PlayerPrefs.GetInt("ZapatosSeleccionada", -1);
        int corbataIndex = PlayerPrefs.GetInt("CorbataSeleccionada", -1);

        if (cabezaIndex >= 0 && cabezaIndex < imagenesCabeza.Count && imagenCabezaSeleccionada != null)
        {
            imagenCabezaSeleccionada.sprite = imagenesCabeza[cabezaIndex];
        }

        if (cuerpoIndex >= 0 && cuerpoIndex < imagenesCuerpo.Count && imagenCuerpoSeleccionada != null)
        {
            imagenCuerpoSeleccionada.sprite = imagenesCuerpo[cuerpoIndex];
        }

        if (zapatosIndex >= 0 && zapatosIndex < imagenesZapatos.Count && imagenZapatosSeleccionada != null)
        {
            imagenZapatosSeleccionada.sprite = imagenesZapatos[zapatosIndex];
        }

        if (corbataIndex >= 0 && corbataIndex < imagenesCorbata.Count && imagenCorbataSeleccionada != null)
        {
            imagenCorbataSeleccionada.sprite = imagenesCorbata[corbataIndex];
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
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey("inventario"))
                {
                    Dictionary<string, object> inventario = (Dictionary<string, object>)data["inventario"];

                    foreach (Transform item in contenedorCabeza)
                    {
                        string claveBD = "cabeza_" + item.name.Split('_')[1];
                        bool desbloqueado = inventario.ContainsKey(claveBD) && (bool)inventario[claveBD];
                        ConfigurarItem(item.gameObject, desbloqueado, claveBD, "cabeza");
                    }

                    foreach (Transform item in contenedorCuerpo)
                    {
                        string claveBD = "cuerpo_" + item.name.Split('_')[1];
                        bool desbloqueado = inventario.ContainsKey(claveBD) && (bool)inventario[claveBD];
                        ConfigurarItem(item.gameObject, desbloqueado, claveBD, "cuerpo");
                    }

                    foreach (Transform item in contenedorZapatos)
                    {
                        string claveBD = "zapatos_" + item.name.Split('_')[1];
                        bool desbloqueado = inventario.ContainsKey(claveBD) && (bool)inventario[claveBD];
                        ConfigurarItem(item.gameObject, desbloqueado, claveBD, "zapatos");
                    }

                    foreach (Transform item in contenedorCorbata)
                    {
                        string claveBD = "corbata_" + item.name.Split('_')[1];
                        bool desbloqueado = inventario.ContainsKey(claveBD) && (bool)inventario[claveBD];
                        ConfigurarItem(item.gameObject, desbloqueado, claveBD, "corbata");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar datos: " + e.Message);
            ConfigurarItemsSinConexion();
        }
    }

    void ConfigurarItem(GameObject item, bool desbloqueado, string claveBD, string tipo)
    {
        Transform pruebaTransform = item.transform.Find("prueba");
        Image img = pruebaTransform.GetComponent<Image>();
        TextMeshProUGUI texto = item.GetComponentInChildren<TextMeshProUGUI>();
        
        if (img == null || texto == null) return;

        int index = int.Parse(claveBD.Split('_')[1]) - 1;
        
        if (desbloqueado)
        {
            img.color = Color.white;
            texto.text = nombresItems.ContainsKey(claveBD) ? nombresItems[claveBD] : "Equipar";
        }
        else
        {
            img.color = new Color(1f, 1f, 1f, 0.5f);
            int precio = preciosItems.ContainsKey(claveBD) ? preciosItems[claveBD] : 50;
            texto.text = $"${precio}";
        }

        Button btn = item.GetComponent<Button>();
        if (btn == null) btn = item.AddComponent<Button>();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            if (desbloqueado)
            {
                SeleccionarItem(index, tipo);
                texto.text = nombresItems.ContainsKey(claveBD) ? nombresItems[claveBD] : "Equipado";
            }
            else
            {
                MostrarPanelCompra(claveBD, item, tipo);
            }
        });
    }

    void SeleccionarItem(int index, string tipo)
    {
        switch (tipo)
        {
            case "cabeza":
                if (index < imagenesCabeza.Count && imagenCabezaSeleccionada != null)
                {
                    imagenCabezaSeleccionada.sprite = imagenesCabeza[index];
                    PlayerPrefs.SetInt("CabezaSeleccionada", index);
                }
                break;
                
            case "cuerpo":
                if (index < imagenesCuerpo.Count && imagenCuerpoSeleccionada != null)
                {
                    imagenCuerpoSeleccionada.sprite = imagenesCuerpo[index];
                    PlayerPrefs.SetInt("CuerpoSeleccionada", index);
                }
                break;
                
            case "zapatos":
                if (index < imagenesZapatos.Count && imagenZapatosSeleccionada != null)
                {
                    imagenZapatosSeleccionada.sprite = imagenesZapatos[index];
                    PlayerPrefs.SetInt("ZapatosSeleccionada", index);
                }
                break;
                
            case "corbata":
                if (index < imagenesCorbata.Count && imagenCorbataSeleccionada != null)
                {
                    imagenCorbataSeleccionada.sprite = imagenesCorbata[index];
                    PlayerPrefs.SetInt("CorbataSeleccionada", index);
                }
                break;
        }
        PlayerPrefs.Save();
    }

    void MostrarPanelCompra(string claveBD, GameObject item, string tipo)
    {
        panelCompra.SetActive(true);
        
        string nombreItem = nombresItems.ContainsKey(claveBD) ? nombresItems[claveBD] : "este ítem";
        int precio = preciosItems.ContainsKey(claveBD) ? preciosItems[claveBD] : 50;
        textoPopup.text = $"¿Quieres comprar {nombreItem} por ${precio}?";

        btnComprar.onClick.RemoveAllListeners();
        btnComprar.onClick.AddListener(() => RealizarCompra(claveBD, item, tipo));

        btnCancelar.onClick.RemoveAllListeners();
        btnCancelar.onClick.AddListener(() => panelCompra.SetActive(false));
    }

    async void RealizarCompra(string claveBD, GameObject item, string tipo)
    {
        int precio = preciosItems.ContainsKey(claveBD) ? preciosItems[claveBD] : 50;
        
        if (GameManager.Instance.GameData.coins >= precio)
        {
            GameManager.Instance.SpendCoins(precio);

            try
            {
                DocumentReference docRef = db.Collection("users").Document(user.Email);
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { $"inventario.{claveBD}", true }
                };

                await docRef.UpdateAsync(updates);
                
                ConfigurarItem(item, true, claveBD, tipo);
                panelCompra.SetActive(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al actualizar la BD: " + e.Message);
            }
        }
        else
        {
            Debug.Log("No tienes suficientes monedas");
        }
    }

    void ConfigurarItemsSinConexion()
    {
        int i = 1;
        foreach (Transform item in contenedorCabeza)
        {
            ConfigurarItem(item.gameObject, false, $"cabeza_{i}", "cabeza");
            i++;
        }

        i = 1;
        foreach (Transform item in contenedorCuerpo)
        {
            ConfigurarItem(item.gameObject, false, $"cuerpo_{i}", "cuerpo");
            i++;
        }

        i = 1;
        foreach (Transform item in contenedorZapatos)
        {
            ConfigurarItem(item.gameObject, false, $"zapatos_{i}", "zapatos");
            i++;
        }

        i = 1;
        foreach (Transform item in contenedorCorbata)
        {
            ConfigurarItem(item.gameObject, false, $"corbata_{i}", "corbata");
            i++;
        }
    }
}