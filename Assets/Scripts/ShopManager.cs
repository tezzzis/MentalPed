
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public GameObject shopItemPrefab;
    public FurnitureData[] itemsEnVenta;
    public List<FurnitureData> itemsComprados = new List<FurnitureData>();

    public GameObject muebleShopPanel;
    public GameObject ropaShopPanel;
    public Transform muebleContent;
    public Transform ropaContent;
    public TMP_Text textoMonedas;
    [Header("Button Sprites")]
    public Sprite muebleActiveSprite;
    public Sprite muebleInactiveSprite;
    public Sprite ropaActiveSprite;
    public Sprite ropaInactiveSprite;
    private Button categoryMuebleButton;
    private Button categoryRopaButton;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "tienda")
        {
            muebleShopPanel = GameObject.Find("MuebleShopPanel");
            ropaShopPanel = GameObject.Find("RopaShopPanel");
            muebleContent = GameObject.Find("MuebleShopPanel").transform;
            ropaContent = GameObject.Find("RopaShopPanel").transform;
            textoMonedas = GameObject.Find("TextoMonedas").GetComponent<TMP_Text>();

           
            categoryMuebleButton = GameObject.Find("BotonMueblesTienda").GetComponent<Button>();
            categoryRopaButton = GameObject.Find("BotonRopaTienda").GetComponent<Button>();

         
            categoryMuebleButton.onClick.AddListener(() => ShowShopCategory(ItemCategory.Mueble));
            categoryRopaButton.onClick.AddListener(() => ShowShopCategory(ItemCategory.Ropa));

          
            GenerateShopItems();
            ActualizarMonedasUI();
            ShowShopCategory(ItemCategory.Mueble);
        }
    }

    void GenerateShopItems()
    {
        foreach (var item in itemsEnVenta)
        {
            if (!itemsComprados.Contains(item))
            {
                Transform parent = item.category == ItemCategory.Mueble ? muebleContent : ropaContent;
                GameObject newItem = Instantiate(shopItemPrefab, parent);
                newItem.GetComponent<ShopItemUI>().Configurar(item);
            }
        }
    }

    void ShowShopCategory(ItemCategory category)
    {
        
        muebleContent.gameObject.SetActive(category == ItemCategory.Mueble);
        ropaContent.gameObject.SetActive(category == ItemCategory.Ropa);

        
        categoryMuebleButton.image.sprite = category == ItemCategory.Mueble
            ? muebleActiveSprite
            : muebleInactiveSprite;

        categoryRopaButton.image.sprite = category == ItemCategory.Ropa
            ? ropaActiveSprite
            : ropaInactiveSprite;
    }

    public void ComprarItem(FurnitureData item)
    {
        if (!itemsComprados.Contains(item) && GameManager.Instance.GameData.coins >= item.precio)
        {
            
            GameManager.Instance.GameData.coins -= item.precio;
            GameManager.Instance.SaveGameData();

            itemsComprados.Add(item);
            InventoryManager.Instance.AgregarAlInventario(item);
            ActualizarMonedasUI();

           
            Transform parent = item.category == ItemCategory.Mueble ? muebleContent : ropaContent;
            foreach (Transform child in parent)
            {
                ShopItemUI itemUI = child.GetComponent<ShopItemUI>();
                if (itemUI != null && itemUI.data == item)
                {
                    child.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    void ActualizarMonedasUI() => textoMonedas.text = $"{GameManager.Instance.GameData.coins}";
}