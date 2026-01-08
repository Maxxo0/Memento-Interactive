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

    [Header("Hide Spot")]
    public float hideEnterSpeed = 12f;

    bool isHidden = false;
    HideSpor currentHideSpot;
    CharacterController playerCC;

    bool inspecting = false;
    GameObject currentInstance;
    InteractableItem currentItem;

    void Start()
    {
        if (inspectCanvas != null)
        {
            inspectCanvas.SetActive(false);
        }
        if (inspectCanvas != null) inspectCanvas.SetActive(false);

        if (playerController != null)
            playerCC = playerController.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isHidden)
        {
            if (Input.GetKeyDown(interactKey))
                SalirEscondite();
            return;
        }

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
        /*Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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
        }*/
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            // 1) ¿Es un hide spot?
            HideSpor hideSpor = hit.collider.GetComponentInParent<HideSpor>();
            if (hideSpor != null)
            {
                // (Opcional) filtro centro de pantalla para que sea más estricto
                Vector3 vp = playerCamera.WorldToViewportPoint(hit.point);
                float dx = Mathf.Abs(vp.x - 0.5f);
                float dy = Mathf.Abs(vp.y - 0.5f);
                if (dx > 0.15f || dy > 0.15f) return;

                if (Input.GetKeyDown(interactKey))
                    EntrarEscondite(hideSpor);

                return; // importante: si es hide, no sigas con item inspect
            }

            // 2) ¿Es un objeto inspeccionable?
            InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
            if (item != null && Input.GetKeyDown(interactKey))
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

    void EntrarEscondite(HideSpor spot)
    {
        if (spot == null || spot.puntoEntrar == null || playerController == null)
            return;

        isHidden = true;
        currentHideSpot = spot;

        if (spot.bloquearMovimiento && playerController != null)
            playerController.bloquearMovimiento = true;


        if (playerCC != null) playerCC.enabled = false;

        Transform p = spot.puntoEntrar;
        playerController.transform.SetPositionAndRotation(p.position, p.rotation);

        if (playerCC != null) playerCC.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SalirEscondite()
    {
        if (currentHideSpot == null || playerController == null)
            return;

        if (playerCC != null) playerCC.enabled = false;

        Transform p = currentHideSpot.puntoSalir != null ? currentHideSpot.puntoSalir : currentHideSpot.puntoEntrar;
        playerController.transform.SetPositionAndRotation(p.position, p.rotation);

        if (playerCC != null) playerCC.enabled = true;

        playerController.bloquearMovimiento = false;

        if (currentHideSpot.bloquearMovimiento && playerController != null)
            playerController.enabled = true;

        isHidden = false;
        currentHideSpot = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerController.ForzarDePie();
    }

}
