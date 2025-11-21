using UnityEngine;

public class ObjectInspectManager : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;   
    public Camera playerCamera;               
    public Transform inspectAnchor;           
    public GameObject inspectBackgroundUI;      

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0;  

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
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();

            if (item != null)
            {

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

        if (playerController != null)
            playerController.enabled = false;

        if (inspectBackgroundUI != null)
            inspectBackgroundUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentInstance = Instantiate(
            item.inspectPrefab,
            inspectAnchor.position,
            inspectAnchor.rotation,
            inspectAnchor
        );


    }

    void TerminarInspeccion()
    {
        inspecting = false;

        if (playerController != null)
            playerController.enabled = true;

        if (inspectBackgroundUI != null)
            inspectBackgroundUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        currentInstance.transform.Rotate(playerCamera.transform.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        currentInstance.transform.Rotate(playerCamera.transform.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
    }
}
