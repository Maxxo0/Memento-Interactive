using UnityEngine;
using UnityEngine.UI;

public enum SlotGroup { Dynamic, Key }

public class InventorySlotUI : MonoBehaviour
{
    public SlotGroup group;
    public int index;

    public Image icon; 

    void Awake()
    {
        if (icon == null)
        {
            var t = transform.Find("Icon");
            if (t != null) icon = t.GetComponent<Image>();
        }

        SetEmpty();
    }

    public void SetEmpty()
    {
        if (icon != null)
        {
            icon.enabled = false;
            icon.sprite = null;
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null)
        {
            icon.enabled = sprite != null;
            icon.sprite = sprite;
        }
    }
}