using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Modelo a inspeccionar")]
    public GameObject inspectPrefab;     

    [Header("Opcional (por si luego quieres UI de texto)")]
    public string itemName;
    [TextArea]
    public string description;
}
