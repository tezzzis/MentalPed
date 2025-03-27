using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ItemCategory { Mueble, Ropa }

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public Transform muebleContent;  
    public Transform ropaContent;    
    public GameObject mainPanel;
    public GameObject slotPrefab;
    [Header("Buttons")]
    public Button abrirMainPanel;
    public Button categoryMuebleButton;
    public Button categoryRopaButton;
    public Button closeButton;
    [Header("Button Sprites")]
    public Sprite muebleActiveSprite;
    public Sprite muebleInactiveSprite;
    public Sprite ropaActiveSprite;
    public Sprite ropaInactiveSprite;

    // Data
    [Header("Data")]
    public List<InventorySlotUI> slots = new List<InventorySlotUI>();
    public List<FurnitureData> inventoryItems = new List<FurnitureData>();
    public DraggableFurniture miScript;

    [Header("Control Cámara")]
    public VerticalScrollController cameraController;

    private bool isInitialized = false;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "main")
        {
            StartCoroutine(InitializeAfterSceneLoad());
        }
        else
        {
            isInitialized = false;
        }
    }

    private IEnumerator InitializeAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.1f); 
        yield return new WaitForEndOfFrame();

        InitializeUIReferences();

        // Verificación crítica de referencias
        if (muebleContent == null || ropaContent == null)
        {
            Debug.LogError("Error crítico: Contenedores no encontrados. Verifica:");
            Debug.LogError("- Nombres exactos: MuebleContent y RopaContent");
            Debug.LogError("- Que existen en la escena 'main'");
            yield break;
        }

        RefreshInventoryUI();
        ClosePanel();
        isInitialized = true;
    }



    private void InitializeUIReferences()
    {
        
        mainPanel = FindInactiveObject("MainInventoryPanel");
        GameObject cameraControllerObj = FindInactiveObject("Main Camera");
        cameraController = cameraControllerObj.GetComponent<VerticalScrollController>();


        muebleContent = FindInactiveObject("MuebleContent")?.transform;
        ropaContent = FindInactiveObject("RopaContent")?.transform;

       
        GameObject muebleSubPanel = muebleContent?.gameObject;
        GameObject ropaSubPanel = ropaContent?.gameObject;

       
        closeButton = FindInactiveObject("BotonCerrarInventario")?.GetComponent<Button>();
        categoryMuebleButton = FindInactiveObject("BotonMuebles")?.GetComponent<Button>();
        categoryRopaButton = FindInactiveObject("BotonRopa")?.GetComponent<Button>();
        abrirMainPanel = FindInactiveObject("BotonAbrirInventario")?.GetComponent<Button>();

       
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);

        categoryMuebleButton.onClick.RemoveAllListeners();
        categoryMuebleButton.onClick.AddListener(() => ShowCategory(ItemCategory.Mueble));

        categoryRopaButton.onClick.RemoveAllListeners();
        categoryRopaButton.onClick.AddListener(() => ShowCategory(ItemCategory.Ropa));

        abrirMainPanel.onClick.RemoveAllListeners();
        abrirMainPanel.onClick.AddListener(ActivePanel);

        DebugUIState();
    }
    private GameObject FindInactiveObject(string name)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == name) return obj;

            Transform[] children = obj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name) return child.gameObject;
            }
        }
        return null;
    }
    private void DebugUIState()
    {
        Debug.Log($"Mueble Content: {(muebleContent != null ? muebleContent.name + " (Active: " + muebleContent.gameObject.activeInHierarchy + ")" : "null")}");
        Debug.Log($"Ropa Content: {(ropaContent != null ? ropaContent.name + " (Active: " + ropaContent.gameObject.activeInHierarchy + ")" : "null")}");
    }



    private Transform GetParentByCategory(ItemCategory category)
    {
        return category == ItemCategory.Mueble ? muebleContent : ropaContent;
    }

    public void AgregarAlInventario(FurnitureData nuevoItem)
    {
        if (!inventoryItems.Contains(nuevoItem))
        {
            inventoryItems.Add(nuevoItem);
            if (isInitialized)
            {
                CreateSlot(nuevoItem);
            }
        }
    }

    private void CreateSlot(FurnitureData item)
    {
        var parent = GetParentByCategory(item.category);
        if (parent == null)
        {
            Debug.LogError($"Parent container not found for category: {item.category}");
            return;
        }

        var slot = Instantiate(slotPrefab, parent);
        var slotUI = slot.GetComponent<InventorySlotUI>();
        slotUI.Setup(item);
        slots.Add(slotUI);
    }

    private void RefreshInventoryUI()
    {
        if (muebleContent == null || ropaContent == null)
        {
            Debug.LogError("Contenedores no inicializados!");
            return;
        }

        ClearSlots();
        foreach (var item in inventoryItems)
        {
            CreateSlot(item);
        }
    }

    private void ClearSlots()
    {
        
        if (muebleContent != null)
        {
            foreach (Transform child in muebleContent)
            {
                if (child != null) Destroy(child.gameObject);
            }
        }

        if (ropaContent != null)
        {
            foreach (Transform child in ropaContent)
            {
                if (child != null) Destroy(child.gameObject);
            }
        }

        slots.Clear();
    }
    void ShowCategory(ItemCategory category)
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
    public void SelectFurniture(FurnitureData data)
    {
        var slot = slots.FirstOrDefault(s =>
            s.data == data &&
            s.gameObject.activeSelf &&
            IsSlotInActiveCategory(s)
        );

        if (slot != null)
        {
            slot.gameObject.SetActive(false);
            var newFurniture = Instantiate(data.prefab);
            newFurniture.GetComponent<DraggableFurniture>().Initialize(data);
        }
    }

    private bool IsSlotInActiveCategory(InventorySlotUI slot)
    {
       
        return (slot.data.category == ItemCategory.Mueble && muebleContent.gameObject.activeSelf) ||
               (slot.data.category == ItemCategory.Ropa && ropaContent.gameObject.activeSelf);
    }

    public void ReturnItem(FurnitureData data)
    {
        var slot = slots.FirstOrDefault(s => s.data == data);
        if (slot != null)
        {
            slot.gameObject.SetActive(true);
            slot.transform.SetParent(GetParentByCategory(data.category));
        }
    }

    public void ActivePanel()
    {
        if (mainPanel == null) InitializeUIReferences();
        mainPanel.SetActive(true);
        cameraController.enabled = false;
        miScript.enabled = true;
        ShowCategory(ItemCategory.Mueble);
    }

    public void ClosePanel()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
            miScript.enabled = false;
            cameraController.enabled = true;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}