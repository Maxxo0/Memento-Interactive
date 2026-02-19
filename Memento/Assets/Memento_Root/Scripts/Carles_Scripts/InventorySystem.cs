using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Capacidad")]
    public int dynamicCapacity = 4;
    public int keyCapacity = 4;

    [Header("Slots UI")]
    public List<InventorySlotUI> dynamicSlotsUI = new();
    public List<InventorySlotUI> keySlotsUI = new();

    ItemData[] dynamicItems;
    ItemData[] keyItems;

    void Awake()
    {
        dynamicItems = new ItemData[dynamicCapacity];
        keyItems = new ItemData[keyCapacity];

        RefreshUI();
    }

    public bool AddItem(ItemData item)
    {
        Debug.Log($"[InventorySystem] AddItem en instancia: {name} | ID: {GetInstanceID()}");
        if (item == null) return false;

        if (item.type == ItemType.Dynamic)
            return AddToArray(dynamicItems, item);
        else
            return AddToArray(keyItems, item);
    }

    bool AddToArray(ItemData[] arr, ItemData item)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null)
            {
                arr[i] = item;

                Debug.Log($"Añadido {item.name} en índice {i} ({item.type})");

                RefreshUI();
                return true;
            }
        }

        Debug.Log("Inventario lleno");
        return false;

    }

    public void RefreshUI()
    {
        for (int i = 0; i < dynamicSlotsUI.Count; i++)
        {
            if (i < dynamicItems.Length && dynamicItems[i] != null)
                dynamicSlotsUI[i].SetIcon(dynamicItems[i].icon);
            else
                dynamicSlotsUI[i].SetEmpty();
        }

        for (int i = 0; i < keySlotsUI.Count; i++)
        {
            if (i < keyItems.Length && keyItems[i] != null)
                keySlotsUI[i].SetIcon(keyItems[i].icon);
            else
                keySlotsUI[i].SetEmpty();
        }
    }

    /*[Header("DEBUG")]
    public ItemData testDynamic;
    public ItemData testKey;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddItem(testDynamic);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            AddItem(testKey);
    }*/

    public void ForceRefreshUI()
    {
        RefreshUI();
    }
}
