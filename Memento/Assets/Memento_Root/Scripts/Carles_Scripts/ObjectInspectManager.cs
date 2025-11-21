using UnityEngine;

public class ObjectInspectManager : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;   // tu script de movimiento
    public Camera playerCamera;                 // Main Camera
    public Transform inspectAnchor;             // el vacío delante de la cámara
    public GameObject inspectBackgroundUI;      // panel negro del Canvas

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0;    // por defecto, todo

    [Header("Inspección")]
    public float rotationSpeed = 200f;

    bool inspecting = false;
    GameObject currentInstance;
    InteractableItem currentItem;

    void Update()
    {
        if (!inspecting)
        {
            DetectarInteraccion();
        }
        else
        {
            RotarObjeto();

            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
            {
                TerminarInspeccion();
            }
        }
    }

    void DetectarInteraccion()
    {
        // Ray desde el centro de la pantalla
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();

            if (item != null)
            {
                // Aquí podrías mostrar un "Pulsa E para inspeccionar" en UI

                if (Input.GetKeyDown(interactKey))
                {
                    EmpezarInspeccion(item);
                }
            }
        }
    }

    void EmpezarInspeccion(InteractableItem item)
    {
        if (item.inspectPrefab == null || inspectAnchor == null)
        {
            Debug.LogWarning("Falta inspectPrefab o inspectAnchor en ObjectInspectManager.");
            return;
        }

        inspecting = true;
        currentItem = item;

        // Desactivar movimiento del jugador
        if (playerController != null)
            playerController.enabled = false;

        // Activar UI negro
        if (inspectBackgroundUI != null)
            inspectBackgroundUI.SetActive(true);

        // Soltar ratón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Instanciar modelo como hijo del anchor
        currentInstance = Instantiate(
            item.inspectPrefab,
            inspectAnchor.position,
            inspectAnchor.rotation,
            inspectAnchor
        );

        // Opcional: desactivar el mesh del objeto original (como si lo cogieras)
        // item.gameObject.SetActive(false);
    }

    void TerminarInspeccion()
    {
        inspecting = false;

        // Reactivar movimiento
        if (playerController != null)
            playerController.enabled = true;

        // Apagar UI negro
        if (inspectBackgroundUI != null)
            inspectBackgroundUI.SetActive(false);

        // Volver a bloquear ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Destruir modelo inspeccionado
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = null;
        currentItem = null;
    }

    void RotarObjeto()
    {
        if (currentInstance == null)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotamos alrededor de los ejes de la cámara para que se sienta natural
        currentInstance.transform.Rotate(playerCamera.transform.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        currentInstance.transform.Rotate(playerCamera.transform.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
    }
}
