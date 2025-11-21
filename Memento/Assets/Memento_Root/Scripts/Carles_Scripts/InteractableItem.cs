using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Modelo a inspeccionar")]
    public GameObject inspectPrefab;     // el modelo bonito para la inspección

    [Header("Opcional (por si luego quieres UI de texto)")]
    public string itemName;
    [TextArea]
    public string description;
}
