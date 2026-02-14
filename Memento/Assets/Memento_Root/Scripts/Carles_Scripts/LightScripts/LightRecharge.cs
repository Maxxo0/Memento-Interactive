using UnityEngine;

public class LightRecharge : MonoBehaviour
{
    [Header("Recarga total disponible")]
    public float cargaTotal = 0.20f; 
    public float curacionPorSegundo = 0.004f;

    [Header("Opciones")]
    public bool seConsume = true; 

    float restante;

    void Awake()
    {
        restante = cargaTotal;
    }

    void OnTriggerStay(Collider other)
    {
        SanityController sanity = other.GetComponentInParent<SanityController>();
        if (sanity == null) return;

        if (restante <= 0f) return;

        float curar = curacionPorSegundo * Time.deltaTime;

        if (seConsume)
            curar = Mathf.Min(curar, restante);

        sanity.RecuperarCordura(curar);

        if (seConsume)
        {
            restante -= curar;
            if (restante <= 0f)
            {
                restante = 0f;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entró: " + other.name);
    }
}
