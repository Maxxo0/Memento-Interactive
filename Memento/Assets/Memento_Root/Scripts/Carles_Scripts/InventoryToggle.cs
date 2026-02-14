using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] GameObject inventoryRoot;
    [SerializeField] KeyCode toggleKey = KeyCode.I;

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
    }
}
