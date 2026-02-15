using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Modelo a inspeccionar")]
    public GameObject inspectPrefab;

    [Header("Inventario (si es recogible)")]
    public bool canPickup = true;
    public ItemData itemData;  

    [Header("Optional")]
    public string itemName;
    [TextArea] public string description;
}
