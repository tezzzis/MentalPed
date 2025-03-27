using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] Image icono;
    [SerializeField] Button botonComprar;

    public FurnitureData data;

    public void Setup(FurnitureData item)
    {
        data = item;
        icono.sprite = item.icon;

        
        botonComprar.onClick.RemoveAllListeners();
        
        botonComprar.onClick.AddListener(() => InventoryManager.Instance.SelectFurniture(data));
    }

   
}