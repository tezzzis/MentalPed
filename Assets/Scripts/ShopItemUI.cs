using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] Image icono;
    [SerializeField] TMP_Text precioTexto;
    [SerializeField] Button botonComprar;

    public FurnitureData data;

    public void Configurar(FurnitureData item)
    {
        data = item;
        icono.sprite = item.icon;
        precioTexto.text = $"{item.precio}G";
        botonComprar.onClick.AddListener(Comprar);
    }

    void Comprar() => ShopManager.Instance.ComprarItem(data);
}