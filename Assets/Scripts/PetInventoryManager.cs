using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class PetInventoryManager : MonoBehaviour
{
    public Transform contenedorPartes; // Contenedor de los items de partes
    public GameObject panelCompra;
    public TextMeshProUGUI textoPopup;
    public Button btnComprar, btnCancelar;

    // Referencias a los componentes de sprite de la mascota en la escena main
    public Image cabezaMascota;
    public Image orejasMascota;
    public Image caraMascota;
    public Image piesMascota;

    private FirebaseFirestore db;
    private FirebaseUser user;
    private string categoriaActual; // Para guardar la categoría de la parte seleccionada
    private GameObject itemSeleccionado; // Item actual seleccionado

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = FirebaseAuth.DefaultInstance.CurrentUser;

        panelCompra.SetActive(false);
        CargarPartesGuardadas();

        if (user != null) CargarEstadoInventario();
        else ConfigurarItemsSinConexion();
    }
    void ConfigurarItemsSinConexion()
    {
    }
    void CargarPartesGuardadas()
    {
        // Cargar sprites seleccionados de PlayerPrefs
        if (PlayerPrefs.HasKey("cabeza"))
            cabezaMascota.sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("cabeza"));

        if (PlayerPrefs.HasKey("orejas"))
            orejasMascota.sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("orejas"));

        // Repetir para otras partes...
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

                if (data.ContainsKey("pet_parts"))
                {
                    Dictionary<string, object> partes = (Dictionary<string, object>)data["pet_parts"];

                    foreach (Transform item in contenedorPartes)
                    {
                        PetPartItem parteItem = item.GetComponent<PetPartItem>();
                        if (parteItem != null)
                        {
                            string categoria = parteItem.partCategory;
                            string nombreParte = parteItem.partName;

                            if (partes.ContainsKey(categoria))
                            {
                                Dictionary<string, object> categoriaParts =
                                    (Dictionary<string, object>)partes[categoria];

                                bool desbloqueado = categoriaParts.ContainsKey(nombreParte) &&
                                                   (bool)categoriaParts[nombreParte];

                                ConfigurarItem(item.gameObject, desbloqueado, categoria);
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error cargando inventario: " + e.Message);
        }
    }

    void ConfigurarItem(GameObject item, bool desbloqueado, string categoria)
    {
        PetPartItem parteItem = item.GetComponent<PetPartItem>();
        Image icono = item.transform.Find("Icono").GetComponent<Image>();
        TextMeshProUGUI texto = item.transform.Find("Texto").GetComponent<TextMeshProUGUI>();
        Button btn = item.GetComponent<Button>();

        if (desbloqueado)
        {
            icono.color = Color.white;
            texto.text = parteItem.partName;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SeleccionarParte(parteItem));
        }
        else
        {
            icono.color = Color.gray;
            texto.text = "$" + parteItem.precio;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                categoriaActual = categoria;
                itemSeleccionado = item;
                MostrarPanelCompra(parteItem);
            });
        }
    }

    void SeleccionarParte(PetPartItem parte)
    {
        // Cambiar el sprite correspondiente en la mascota
        switch (parte.partCategory)
        {
            case "cabeza":
                cabezaMascota.sprite = parte.partSprite;
                PlayerPrefs.SetString("cabeza", parte.partSprite.name);
                break;
            case "orejas":
                orejasMascota.sprite = parte.partSprite;
                PlayerPrefs.SetString("orejas", parte.partSprite.name);
                break;
                // Añadir demás casos...
        }

        PlayerPrefs.Save();
    }

    void MostrarPanelCompra(PetPartItem parte)
    {
        panelCompra.SetActive(true);
        textoPopup.text = $"¿Comprar {parte.partName} por ${parte.precio}?";

        btnComprar.onClick.RemoveAllListeners();
        btnComprar.onClick.AddListener(() => RealizarCompra(parte));

        btnCancelar.onClick.RemoveAllListeners();
        btnCancelar.onClick.AddListener(() => panelCompra.SetActive(false));
    }

    async void RealizarCompra(PetPartItem parte)
    {
        if (GameManager.Instance.GameData.coins >= parte.precio)
        {
            GameManager.Instance.SpendCoins(parte.precio);

            try
            {
                DocumentReference docRef = db.Collection("users").Document(user.Email);
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { $"pet_parts.{parte.partCategory}.{parte.partName}", true }
                };

                await docRef.UpdateAsync(updates);
                ConfigurarItem(itemSeleccionado, true, parte.partCategory);
                panelCompra.SetActive(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error comprando: " + e.Message);
            }
        }
    }
}

// Script adicional para adjuntar a los items del inventario
[System.Serializable]
public class PetPartItem : MonoBehaviour
{
    public string partName;
    public string partCategory; // cabeza, orejas, etc.
    public int precio;
    public Sprite partSprite;
}