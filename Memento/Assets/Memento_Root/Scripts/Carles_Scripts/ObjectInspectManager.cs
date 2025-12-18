using UnityEngine;
using UnityEngine.UI;

public class ObjectInspectManager : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;   
    public Camera playerCamera;                
    public Transform inspectAnchor;
    public GameObject inspectCanvas;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0;  

    [Header("Inspección")]
    public float rotationSpeed = 200f;


    bool inspecting = false;
    GameObject currentInstance;
    InteractableItem currentItem;

    void Start()
    {
        if (inspectCanvas != null)
        {
            inspectCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!inspecting)
        {
            DetectarInteraccion();  
        }
        else
        {
            RotarObjeto();;

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
            InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
            if (item == null) return;

            Vector3 vp = playerCamera.WorldToViewportPoint(hit.point);

            bool enPantalla =
                vp.z > 0f &&
                vp.x >= 0f && vp.x <= 1f &&
                vp.y >= 0f && vp.y <= 1f;

            if (!enPantalla) return;
            float dx = Mathf.Abs(vp.x - 0.5f);
            float dy = Mathf.Abs(vp.y - 0.5f);

            if (dx > 0.15f || dy > 0.15f) return;

            if (Input.GetKeyDown(interactKey))
            {
                EmpezarInspeccion(item);
            }
        }
    }

    void EmpezarInspeccion(InteractableItem item)
    {
        if (item.inspectPrefab == null || inspectAnchor == null)
        {
            Debug.LogWarning("[Inspect] Falta inspectPrefab o inspectAnchor");
            return;
        }

        inspecting = true;
        currentItem = item;
        currentItem.gameObject.SetActive(false);


        if (playerController != null)
            playerController.enabled = false;

        if (inspectCanvas != null)
        {
            Debug.Log("[Inspect] Activando Canvas de inspección");
            inspectCanvas.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentInstance = Instantiate(
            item.inspectPrefab,
            inspectAnchor.position,
            inspectAnchor.rotation,
            inspectAnchor
        );

        var rb = currentInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void TerminarInspeccion()
    {
        inspecting = false;

        if (playerController != null)
            playerController.enabled = true;

        if (inspectCanvas != null)
        {
            Debug.Log("[Inspect] Apagando Canvas de inspección");
            inspectCanvas.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentInstance != null)
            Destroy(currentInstance);

        if (currentItem != null)
            currentItem.gameObject.SetActive(true);

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
