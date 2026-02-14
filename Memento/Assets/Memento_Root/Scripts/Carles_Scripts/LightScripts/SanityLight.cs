using UnityEngine;

[RequireComponent(typeof(Light))]
public class SanityLight : MonoBehaviour
{
    [Header("Cordura")]
    public float corduraDisponible = 0.3f;  
    public float velocidadCuracion = 0.05f;  

    Light luz;
    bool consumida = false;

    void Awake()
    {
        luz = GetComponent<Light>();
        if (luz == null) luz = GetComponentInParent<Light>();

        Debug.Log($"[SanityLight] Encontrada Light: {(luz != null ? luz.name : "NULL")} en {gameObject.name}");
    }

    public float Curar(float deltaTime)
    {
        if (consumida || corduraDisponible <= 0f)
            return 0f;

        float cantidad = velocidadCuracion * deltaTime;
        cantidad = Mathf.Min(cantidad, corduraDisponible);

        corduraDisponible -= cantidad;

        if (corduraDisponible <= 0f)
            Apagar();

        return cantidad;
    }

    void Apagar()
    {
        Debug.Log("[SanityLight] Luz agotada -> apagando");

        if (luz != null)
        {
            luz.enabled = false;
            Debug.Log($"[SanityLight] Light.enabled = {luz.enabled} (debería ser false)");
        }
        else
        {
            Debug.LogWarning("[SanityLight] No hay Light para apagar.");
        }
    }
}
