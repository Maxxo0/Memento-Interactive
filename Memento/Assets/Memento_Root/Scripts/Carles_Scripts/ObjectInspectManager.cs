using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectInspectManager : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;   
    public Camera playerCamera;                
    public Transform inspectAnchor;
    public GameObject inspectCanvas;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 1.4f;
    public LayerMask interactLayerMask = ~0;

    [Header("UI Prompt")]
    public GameObject promptGO;  
    public TMP_Text promptText;       
    public string textoAgacharse = "Agacharse";

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

    [Header("Hide Transition")]
    public float duracionTransicion = 0.45f;
    public AnimationCurve curvaMovimiento = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Inventario")]
    public InventorySystem inventory;

    [Header("UI Inspección")]
    public GameObject pickupHintGO; 
    public TMP_Text pickupHintText;
    public TMP_Text textoCerrar;
    public TMP_Text textoGuardar;
    public GameObject guardarGO;
    public KeyCode guardarKey = KeyCode.F; 

    bool enTransicion = false;

    void Start()
    {
        if (inspectCanvas != null)
            inspectCanvas.SetActive(false);

        if (promptGO != null)
            promptGO.SetActive(false);
        if (playerController != null)
            playerCC = playerController.GetComponent<CharacterController>();

        if (pickupHintGO != null) pickupHintGO.SetActive(false);
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

            // GUARDAR EN INVENTARIO
            if (Input.GetKeyDown(guardarKey))
            {
                GuardarEnInventario();
                return;
            }

            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
            {
                TerminarInspeccion();
            }

        }

        if (inspecting && currentItem != null && Input.GetKeyDown(KeyCode.F))
        {
            if (inventory != null && currentItem.itemData != null)
            {
                bool ok = inventory.AddItem(currentItem.itemData);

                if (ok)
                {
                    Destroy(currentItem.gameObject); 
                    TerminarInspeccion();
                }
                else
                {
                    Debug.Log("Inventario lleno");
                }
            }
        }
    }

    void DetectarInteraccion()
    {

        if (isHidden || enTransicion)
        {
            if (promptGO != null) promptGO.SetActive(false);
            return;
        }
        bool mostrarPrompt = false;

        Vector3 origenHide = playerController.transform.position + Vector3.up * 0.8f;
        Ray rayHide = new Ray(origenHide, playerController.transform.forward);

        if (Physics.Raycast(rayHide, out RaycastHit hitHide, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            HideSpor hideSpot = hitHide.collider.GetComponentInParent<HideSpor>();
            if (hideSpot != null)
            {

                mostrarPrompt = true;

                if (Input.GetKeyDown(interactKey))
                {

                    EntrarEscondite(hideSpot);
                    return;
                }
            }
        }

        if (promptGO != null)
            promptGO.SetActive(mostrarPrompt);

        if (mostrarPrompt && promptText != null)
            promptText.text = textoAgacharse;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
            if (item != null && Input.GetKeyDown(interactKey))
                EmpezarInspeccion(item);
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

        if (textoCerrar != null)
            textoCerrar.text = "Cerrar (E / Esc)";

        bool puedeGuardar = item != null && item.itemData != null;

        if (guardarGO != null)
            guardarGO.SetActive(puedeGuardar);

        if (puedeGuardar && textoGuardar != null)
            textoGuardar.text = $"Guardar ({guardarKey})";

        bool canStore = item != null && item.canPickup && item.itemData != null;

        if (pickupHintGO != null)
            pickupHintGO.SetActive(canStore);

        if (canStore && pickupHintText != null)
            pickupHintText.text = $"Guardar ({guardarKey})";

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
       
        if (spot == null || enTransicion) return;

        StartCoroutine(EntrarEsconditeSuave(spot));
        if (promptGO != null) promptGO.SetActive(false);
    }

    void SalirEscondite()
    {
        
        if (currentHideSpot == null || enTransicion) return;

        StartCoroutine(SalirEsconditeSuave());
        if (promptGO != null) promptGO.SetActive(false);
    }

    IEnumerator EntrarEsconditeSuave(HideSpor spot)
    {
        enTransicion = true;
        isHidden = true;
        currentHideSpot = spot;

        playerController.bloquearMovimiento = true;

        if (playerCC != null)
            playerCC.enabled = false;

        Transform target = spot.puntoEntrar;

        Vector3 posInicial = playerController.transform.position;
        Quaternion rotInicial = playerController.transform.rotation;

        Vector3 posFinal = target.position;
        Quaternion rotFinal = target.rotation;

        float t = 0f;
        while (t < duracionTransicion)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duracionTransicion);
            float k = curvaMovimiento.Evaluate(n);

            playerController.transform.position =
                Vector3.Lerp(posInicial, posFinal, k);

            playerController.transform.rotation =
                Quaternion.Slerp(rotInicial, rotFinal, k);

            yield return null;
        }

        playerController.transform.SetPositionAndRotation(posFinal, rotFinal);

        if (playerCC != null)
            playerCC.enabled = true;

        enTransicion = false;
    }

    IEnumerator SalirEsconditeSuave()
    {
        enTransicion = true;

        if (playerCC != null)
            playerCC.enabled = false;

        Transform target = currentHideSpot.puntoSalir != null
            ? currentHideSpot.puntoSalir
            : currentHideSpot.puntoEntrar;

        Vector3 posInicial = playerController.transform.position;
        Quaternion rotInicial = playerController.transform.rotation;

        Vector3 posFinal = target.position;
        Quaternion rotFinal = target.rotation;

        float t = 0f;
        while (t < duracionTransicion)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duracionTransicion);
            float k = curvaMovimiento.Evaluate(n);

            playerController.transform.position =
                Vector3.Lerp(posInicial, posFinal, k);

            playerController.transform.rotation =
                Quaternion.Slerp(rotInicial, rotFinal, k);

            yield return null;
        }

        playerController.transform.SetPositionAndRotation(posFinal, rotFinal);

        if (playerCC != null)
            playerCC.enabled = true;

        playerController.bloquearMovimiento = false;
        playerController.ForzarDePie();
        isHidden = false;
        currentHideSpot = null;

        enTransicion = false;
    }

    void GuardarItemActual()
    {
        if (currentItem == null || inventory == null) return;
        if (!currentItem.canPickup || currentItem.itemData == null) return;

        bool ok = inventory.AddItem(currentItem.itemData);
        if (!ok)
        {
            Debug.Log("Inventario lleno");
            return;
        }

        Destroy(currentItem.gameObject);
        inspecting = false;

        if (playerController != null)
            playerController.enabled = true;

        if (inspectCanvas != null)
            inspectCanvas.SetActive(false);

        if (pickupHintGO != null)
            pickupHintGO.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = null;
        currentItem = null;
    }

    void GuardarEnInventario()
    {
        if (inventory == null || currentItem == null)
        {
            Debug.LogWarning("[Inspect] Falta Inventory o currentItem.");
            return;
        }

        if (currentItem.itemData == null)
        {
            Debug.LogWarning("[Inspect] Este objeto no tiene ItemData asignado.");
            return;
        }

        bool ok = inventory.AddItem(currentItem.itemData);
        if (!ok)
        {
            Debug.Log("Inventario lleno, no se puede guardar.");
            return;
        }

        GameObject worldGO = currentItem.gameObject;

        TerminarInspeccion();

        Destroy(worldGO);
    }
}
