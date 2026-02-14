using UnityEngine;
using UnityEngine.UI;

public enum SlotGroup { Dynamic, Key }

public class InventorySlotUI : MonoBehaviour
{
    public SlotGroup group;
    public int index;

    [Header("UI")]
    public Image icon;
    public GameObject emptyVisual; 

    public void SetEmpty()
    {
        if (icon != null)
        {
            icon.enabled = false;
            icon.sprite = null;
        }
        if (emptyVisual != null) emptyVisual.SetActive(true);
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null)
        {
            icon.enabled = sprite != null;
            icon.sprite = sprite;
        }
        if (emptyVisual != null) emptyVisual.SetActive(sprite == null);
    }
}