using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    CharacterController controller;
    Vector3 velocidadVertical;
    float rotacionX = 0f;

    Vector3 camaraPosLocalInicial;
    float contadorHeadbob = 0f;

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
    }

    void Update()
    {
        float rawMouseX = Input.GetAxis("Mouse X");
        float rawMouseY = Input.GetAxis("Mouse Y");

        float mouseX = rawMouseX * sensibilidadRaton * Time.deltaTime;
        float mouseY = rawMouseY * sensibilidadRaton * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        transform.Rotate(Vector3.up * mouseX);

        ActualizarCameraSway(rawMouseX, rawMouseY);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 input = (transform.right * x + transform.forward * z).normalized;
        float intensidadMovimiento = new Vector2(x, z).magnitude;

        controller.Move(input * velocidad * Time.deltaTime);

        if (controller.isGrounded && velocidadVertical.y < 0)
            velocidadVertical.y = -2f;

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);

        ActualizarHeadbob(intensidadMovimiento);
    }

    void ActualizarHeadbob(float intensidadMovimiento)
    {
        if (!usarHeadbob || camara == null)
            return;

        bool enSuelo = controller.isGrounded;

        if (enSuelo && intensidadMovimiento > 0.1f)
        {
            contadorHeadbob += Time.deltaTime * frecuenciaPaso * (0.5f + intensidadMovimiento);

            float bobY = Mathf.Sin(contadorHeadbob) * amplitudPaso;
            float bobX = Mathf.Cos(contadorHeadbob * 0.5f) * amplitudPaso * 0.5f;

            Vector3 objetivo = camaraPosLocalInicial + new Vector3(bobX, bobY, 0f);
            camara.localPosition = Vector3.Lerp(camara.localPosition, objetivo, Time.deltaTime * suavizadoHeadbob);
        }
        else
        {
            contadorHeadbob += Time.deltaTime * frecuenciaIdle;

            float bobY = Mathf.Sin(contadorHeadbob) * amplitudIdle;

            Vector3 objetivo = camaraPosLocalInicial + new Vector3(0f, bobY, 0f);
            camara.localPosition = Vector3.Lerp(camara.localPosition, objetivo, Time.deltaTime * (suavizadoHeadbob * 0.5f));
        }
    }

    void ActualizarCameraSway(float rawMouseX, float rawMouseY)
    {
        if (!usarSway || camara == null)
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
}
