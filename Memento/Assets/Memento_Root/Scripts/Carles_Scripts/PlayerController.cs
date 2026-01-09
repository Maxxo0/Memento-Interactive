using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public bool bloquearMovimiento = false;

    [Header("Movimiento")]
    public float velocidad = 6f;
    public float gravedad = -9.81f;

    [Header("Rotación FPS")]
    public float sensibilidadRaton = 300f;
    public Transform camara;          
    public float limiteVertical = 80f;

    [Header("Headbob (terror estilo P.T.)")]
    public bool usarHeadbob = true;
    public float amplitudPaso = 0.035f;
    public float frecuenciaPaso = 1.4f;
    public float amplitudIdle = 0.006f;
    public float frecuenciaIdle = 0.8f;
    public float suavizadoHeadbob = 12f;

    [Header("Camera Sway (tipo P.T.)")]
    public bool usarSway = true;
    public float swayCantidad = 1.5f;
    public float swayMaxAngulo = 2f;
    public float swaySuavizado = 8f;

    [Header("Crouch (agacharse)")]
    public bool permitirAgacharse = true;
    public KeyCode teclaAgacharse = KeyCode.LeftShift;
    public float alturaAgachado = 1.0f;       
    public float velocidadAgachado = 3f;     
    public float offsetCamaraAgachado = 0.5f; 
    public float suavizadoAgachado = 10f;

    [Header("Zoom (modo P.T.)")]
    public bool usarZoom = true;
    public KeyCode teclaZoom = KeyCode.Mouse1;  
    public float fovNormal = 60f;
    public float fovZoom = 40f;                 
    public float velocidadZoom = 10f;
    public float factorSensibilidadZoom = 0.6f;
    Camera cam;

    CharacterController controller;
    Vector3 velocidadVertical;
    float rotacionX = 0f;

    Vector3 camaraPosLocalInicial;
    float contadorHeadbob = 0f;

    float alturaOriginal;
    Vector3 centroOriginal;
    bool estaAgachado;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camara != null)
            camaraPosLocalInicial = camara.localPosition;

        alturaOriginal = controller.height;
        centroOriginal = controller.center;

        if (camara != null)
        {
            cam = camara.GetComponent<Camera>();
            if (cam != null)
                fovNormal = cam.fieldOfView;   
        }
    }

    void Update()
    {
        bool estaHaciendoZoom = usarZoom && Input.GetKey(teclaZoom);

        float sensibilidadActual = sensibilidadRaton;
        if (estaHaciendoZoom)
            sensibilidadActual *= factorSensibilidadZoom;

        float rawMouseX = Input.GetAxis("Mouse X");
        float rawMouseY = Input.GetAxis("Mouse Y");

        float mouseX = rawMouseX * sensibilidadActual * Time.deltaTime;
        float mouseY = rawMouseY * sensibilidadActual * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        transform.Rotate(Vector3.up * mouseX);

        ActualizarCameraSway(rawMouseX, rawMouseY);
        ActualizarZoom(estaHaciendoZoom);

        ActualizarCrouch();
        if (bloquearMovimiento)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 input = (transform.right * x + transform.forward * z).normalized;
        float intensidadMovimiento = new Vector2(x, z).magnitude;

        float velocidadActual = estaAgachado ? velocidadAgachado : velocidad;
        controller.Move(input * velocidadActual * Time.deltaTime);

        if (controller.isGrounded && velocidadVertical.y < 0)
            velocidadVertical.y = -2f;

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);

        ActualizarHeadbob(intensidadMovimiento);
    }

    void ActualizarCrouch()
    {
        if (!permitirAgacharse)
        {
            estaAgachado = false;
            return;
        }

        bool deseaAgacharse = Input.GetKey(teclaAgacharse);
        estaAgachado = deseaAgacharse;

        float alturaObjetivo = deseaAgacharse ? alturaAgachado : alturaOriginal;
        controller.height = Mathf.Lerp(controller.height, alturaObjetivo, Time.deltaTime * suavizadoAgachado);

        float centroYOriginal = centroOriginal.y;
        float centroYAgachado = alturaAgachado * 0.5f; 

        Vector3 centroActual = controller.center;
        float centroObjetivoY = deseaAgacharse ? centroYAgachado : centroYOriginal;
        centroActual.y = Mathf.Lerp(centroActual.y, centroObjetivoY, Time.deltaTime * suavizadoAgachado);
        controller.center = centroActual;
    }

    void ActualizarHeadbob(float intensidadMovimiento)
    {
        if (!usarHeadbob || camara == null)
            return;

        bool enSuelo = controller.isGrounded;

        Vector3 baseCamara = camaraPosLocalInicial +
                             (estaAgachado ? Vector3.down * offsetCamaraAgachado : Vector3.zero);

        if (enSuelo && intensidadMovimiento > 0.1f)
        {
            contadorHeadbob += Time.deltaTime * frecuenciaPaso * (0.5f + intensidadMovimiento);

            float bobY = Mathf.Sin(contadorHeadbob) * amplitudPaso;
            float bobX = Mathf.Cos(contadorHeadbob * 0.5f) * amplitudPaso * 0.5f;

            Vector3 objetivo = baseCamara + new Vector3(bobX, bobY, 0f);
            camara.localPosition = Vector3.Lerp(camara.localPosition, objetivo, Time.deltaTime * suavizadoHeadbob);
        }
        else
        {
            contadorHeadbob += Time.deltaTime * frecuenciaIdle;

            float bobY = Mathf.Sin(contadorHeadbob) * amplitudIdle;

            Vector3 objetivo = baseCamara + new Vector3(0f, bobY, 0f);
            camara.localPosition = Vector3.Lerp(camara.localPosition, objetivo, Time.deltaTime * (suavizadoHeadbob * 0.5f));
        }
    }

    void ActualizarCameraSway(float rawMouseX, float rawMouseY)
    {
        if (camara == null)
            return;

        if (!usarSway)
        {
            camara.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
            return;
        }

        float swayX = Mathf.Clamp(-rawMouseY * swayCantidad, -swayMaxAngulo, swayMaxAngulo);
        float swayZ = Mathf.Clamp(-rawMouseX * swayCantidad, -swayMaxAngulo, swayMaxAngulo);

        Quaternion rotBase = Quaternion.Euler(rotacionX, 0f, 0f);
        Quaternion rotSway = Quaternion.Euler(swayX, 0f, swayZ);

        Quaternion rotObjetivo = rotBase * rotSway;

        camara.localRotation = Quaternion.Slerp(
            camara.localRotation,
            rotObjetivo,
            Time.deltaTime * swaySuavizado
        );
    }

    void ActualizarZoom(bool estaHaciendoZoom)
    {
        if (!usarZoom || cam == null)
            return;

        float objetivoFOV = estaHaciendoZoom ? fovZoom : fovNormal;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            objetivoFOV,
            Time.deltaTime * velocidadZoom
        );
    }

    public void ForzarDePie()
    {
        estaAgachado = false;
        controller.height = alturaOriginal;
        controller.center = centroOriginal;
    }
}
