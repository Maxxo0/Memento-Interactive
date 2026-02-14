using UnityEngine;

public class SanityLightTrigger : MonoBehaviour
{
    SanityLight sanityLight;

    void Awake()
    {
        sanityLight = GetComponentInParent<SanityLight>();
        if (sanityLight == null)
            Debug.LogWarning("[SanityLightTrigger] No encuentro SanityLight en el padre.");
    }

    void OnTriggerStay(Collider other)
    {
        if (sanityLight == null) return;

        var sanity = other.GetComponentInParent<SanityController>();
        if (sanity == null) return;

        float curado = sanityLight.Curar(Time.deltaTime);

        if (curado > 0f)
            sanity.RecuperarCordura(curado);
    }
}
