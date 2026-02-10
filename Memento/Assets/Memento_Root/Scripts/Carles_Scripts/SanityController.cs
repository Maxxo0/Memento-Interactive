using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityController : MonoBehaviour
{
    [Header("Referencias")]
    public Volume globalVolume;
    public Transform player;             
    public Transform[] enemigos;             

    [Header("Cordura (0 = mal, 1 = bien)")]
    [Range(0f, 1f)] public float cordura = 1f;

    [Header("Ritmos")]
    public float drenajeOscuridad = 0.0025f;
    public float drenajeEnemigo = 0.015f;    
    public float recuperacionLuz = 0.0035f;    

    [Header("Enemigo")]
    public float distanciaPeligro = 8f;    
    public float distanciaMax = 2.5f;       

    [Header("Suavizado")]
    public float suavizadoEfecto = 8f;

    [Header("Drenaje base")]
    public float drenajeBase = 0.0017f;

    int zonasDeLuzDentro = 0;


    DepthOfField dof;
    Vignette vignette;
    ChromaticAberration chroma;
    FilmGrain grain;

    float intensidadActual = 0f; 

    void Awake()
    {
        if (player == null) player = transform;

        if (globalVolume == null)
        {
            Debug.LogError("[Sanity] Falta asignar Global Volume.");
            enabled = false;
            return;
        }

        var profile = globalVolume.profile;
        profile.TryGet(out dof);
        profile.TryGet(out vignette);
        profile.TryGet(out chroma);
        profile.TryGet(out grain);

        if (dof == null || vignette == null || chroma == null)
            Debug.LogWarning("[Sanity] Te faltan overrides (DOF/Vignette/Chromatic) en el Volume Profile.");
    }

    void Update()
    {
        cordura -= drenajeBase * Time.deltaTime;
        bool enLuz = zonasDeLuzDentro > 0;

        float amenaza = CalcularAmenazaEnemigo();
        if (enLuz)
        {
            float drenajePorEnemigo = amenaza * drenajeEnemigo;
            cordura += (recuperacionLuz - drenajePorEnemigo) * Time.deltaTime;
        }
        else
        {
            float drenajeTotal = drenajeOscuridad + (amenaza * drenajeEnemigo);
            cordura -= drenajeTotal * Time.deltaTime;
        }

        cordura = Mathf.Clamp01(cordura);

        float objetivo = 1f - cordura;

        intensidadActual = Mathf.Lerp(intensidadActual, objetivo, Time.deltaTime * suavizadoEfecto);

        AplicarPostProcesado(intensidadActual);
    }

    float CalcularAmenazaEnemigo()
    {
        if (enemigos == null || enemigos.Length == 0) return 0f;

        float minDist = float.MaxValue;
        Vector3 p = player.position;

        foreach (var e in enemigos)
        {
            if (e == null) continue;
            float d = Vector3.Distance(p, e.position);
            if (d < minDist) minDist = d;
        }

        if (minDist > distanciaPeligro) return 0f;


        float t = Mathf.InverseLerp(distanciaPeligro, distanciaMax, minDist);
        return Mathf.Clamp01(t);
    }

    void AplicarPostProcesado(float t)
    {
        if (dof != null)
        {
            dof.active = true;

            dof.mode.value = DepthOfFieldMode.Gaussian;

            dof.gaussianStart.value = Mathf.Lerp(2.5f, 0.4f, t);
            dof.gaussianEnd.value = Mathf.Lerp(6.0f, 1.2f, t);
            dof.gaussianMaxRadius.value = Mathf.Lerp(0.2f, 1.2f, t);
        }

        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.value = Mathf.Lerp(0.15f, 0.55f, t);
            vignette.smoothness.value = Mathf.Lerp(0.25f, 0.6f, t);
        }

        if (chroma != null)
        {
            chroma.active = true;
            chroma.intensity.value = Mathf.Lerp(0.05f, 0.35f, t);
        }

        if (grain != null)
        {
            grain.active = true;
            grain.intensity.value = Mathf.Lerp(0.0f, 0.35f, t);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LightZone>() != null)
            zonasDeLuzDentro++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LightZone>() != null)
            zonasDeLuzDentro = Mathf.Max(0, zonasDeLuzDentro - 1);
    }
    public void RecuperarCordura(float cantidad)
    {
        cordura = Mathf.Clamp01(cordura + cantidad);
    }
}
