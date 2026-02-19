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
    [SerializeField] GameObject inventoryRoot;
    [SerializeField] KeyCode toggleKey = KeyCode.I;
    [SerializeField] InventorySystem inventorySystem;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] PlayerController player;

    [Header("Fade")]
    [SerializeField] float fadeDuration = 0.25f;
    [SerializeField] bool pauseGame = false;

    bool abierto = false;
    float fadeTimer = 0f;

    void Start()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();

        // Animación fade usando unscaledDeltaTime (funciona aunque pausemos)
        float dt = Time.unscaledDeltaTime;
        float objetivo = abierto ? fadeDuration : 0f;

        fadeTimer = Mathf.MoveTowards(fadeTimer, objetivo, dt);
        float alpha = fadeDuration > 0 ? fadeTimer / fadeDuration : (abierto ? 1f : 0f);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = alpha > 0.95f;
            canvasGroup.blocksRaycasts = alpha > 0.95f;
        }
    }

    void Toggle()
    {
        abierto = !abierto;

        if (abierto)
        {
            if (inventorySystem != null)
                inventorySystem.RefreshUI();

            if (player != null)
            {
                player.bloquearMovimiento = true;
                player.bloquearCamara = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseGame)
                Time.timeScale = 0f;
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

            if (pauseGame)
                Time.timeScale = 1f;
        }
    }
}
