using System.Collections;
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
    public float interactDistance = 1.4f;
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

    [Header("Hide Transition")]
    public float duracionTransicion = 0.45f;
    public AnimationCurve curvaMovimiento = AnimationCurve.EaseInOut(0, 0, 1, 1);

    bool enTransicion = false;

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
        Vector3 origenHide = playerController.transform.position + Vector3.up * 0.8f;
        Ray rayHide = new Ray(origenHide, playerController.transform.forward);

        if (Physics.Raycast(
            rayHide,
            out RaycastHit hitHide,
            interactDistance,
            interactLayerMask,
            QueryTriggerInteraction.Collide))
        {
            HideSpor hideSpot = hitHide.collider.GetComponentInParent<HideSpor>();
            if (hideSpot != null)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    EntrarEscondite(hideSpot);
                }
                return; 
            }
        }


        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance,
            interactLayerMask,
            QueryTriggerInteraction.Collide))
        {
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
        /*if (spot == null || spot.puntoEntrar == null || playerController == null)
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
        Cursor.visible = false;*/
        if (spot == null || enTransicion) return;

        StartCoroutine(EntrarEsconditeSuave(spot));
    }

    void SalirEscondite()
    {
        /*if (currentHideSpot == null || playerController == null)
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
        playerController.ForzarDePie();*/
        if (currentHideSpot == null || enTransicion) return;

        StartCoroutine(SalirEsconditeSuave());
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
}
