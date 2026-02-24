using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    /*[SerializeField] GameObject inventoryRoot;
    [SerializeField] KeyCode toggleKey = KeyCode.I;
    [SerializeField] InventorySystem inventory;

    [SerializeField] InventorySystem inventorySystem;

    void Start()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && inventoryRoot != null)
        {
            bool newState = !inventoryRoot.activeSelf;
            inventoryRoot.SetActive(newState);

            if (newState)
            {
                if (inventorySystem != null)
                    Debug.Log($"[InventoryToggle] RefreshUI en instancia: {inventorySystem.name} | ID: {inventorySystem.GetInstanceID()}");
                inventorySystem.RefreshUI();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
        }

    }*/
    [SerializeField] GameObject inventoryRoot;     // InventoryRoot (o BlackInventory)
    [SerializeField] KeyCode toggleKey = KeyCode.I;
    [SerializeField] InventorySystem inventorySystem;
    [SerializeField] PlayerController player;
    [SerializeField] bool pauseGame = false;

    bool abierto;

    void Awake()
    {
        // Si arrancas con InventoryRoot apagado en jerarquía, esto lo detecta bien
        abierto = inventoryRoot != null && inventoryRoot.activeSelf;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    void Toggle()
    {
        if (inventoryRoot == null) return;

        abierto = !inventoryRoot.activeSelf;
        inventoryRoot.SetActive(abierto); // <-- Esto es lo que tú quieres ver en la jerarquía

        if (abierto)
        {
            inventorySystem?.RefreshUI();

            if (player != null)
            {
                player.bloquearMovimiento = true;
                player.bloquearCamara = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseGame) Time.timeScale = 0f;
        }
        else
        {
            if (player != null)
            {
                player.bloquearMovimiento = false;
                player.bloquearCamara = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pauseGame) Time.timeScale = 1f;
        }
    }
}
