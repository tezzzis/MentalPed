// FurnitureData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewFurniture", menuName = "Build System/Furniture Data")]
public class FurnitureData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    public int precio;
    public ItemCategory category;
}