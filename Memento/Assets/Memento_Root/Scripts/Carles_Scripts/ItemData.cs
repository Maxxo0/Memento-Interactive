using UnityEngine;

public enum ItemType { Dynamic, Key }

[CreateAssetMenu(menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public GameObject inventoryPrefab3D; // modelo para mostrar en inventario
    public string itemId;
    public ItemType type; // Dynamic / Key
    public Sprite icon;
}