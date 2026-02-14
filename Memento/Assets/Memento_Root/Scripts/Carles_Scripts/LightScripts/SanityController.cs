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
    [SerializeField] bool resetCorduraAlIniciar = true;
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

    [Header("Curva de amenaza")]
    [Tooltip("Más alto = sube MUY rápido al estar cerca (2-4 suele ir bien).")]
    public float exponenteAmenaza = 3f;

    [Header("Umbral de pánico (efectos fuertes)")]
    [Range(0f, 1f)] public float umbralPanico = 0.4f;
    public float potenciaPanico = 2.5f;               

    int zonasDeLuzDentro = 0;
    SanityLight luzActual;

    DepthOfField dof;
    Vignette vignette;
    ChromaticAberration chroma;
    FilmGrain grain;

    float intensidadActual = 0f; 

    void Awake()
    {
        if (resetCorduraAlIniciar)
            cordura = 1f;

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

        if (luzActual != null)
        {
            float curado = luzActual.Curar(Time.deltaTime);
            cordura = Mathf.Clamp01(cordura + curado);
        }
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
        t = Mathf.Clamp01(t);

        t = Mathf.Pow(t, exponenteAmenaza);

        return t;
    }

    void AplicarPostProcesado(float t)
    {
        const float umbral = 0.45f;

        float corduraActual = 1f - t;               
        float low = Mathf.InverseLerp(umbral, 0f, corduraActual);
        low = Mathf.Clamp01(low);
        low = Mathf.Pow(low, 2.5f);

        if (dof != null)
        {
            // Si cordura >= 0.4 -> NADA de borroso
            if (corduraActual >= umbral)
            {
                dof.active = false;               
                dof.gaussianMaxRadius.value = 0f;   
            }
            else
            {
                dof.active = true;
                dof.mode.value = DepthOfFieldMode.Gaussian;

                dof.gaussianStart.value = Mathf.Lerp(2.5f, 0.4f, low);
                dof.gaussianEnd.value = Mathf.Lerp(6.0f, 1.2f, low);
                dof.gaussianMaxRadius.value = Mathf.Lerp(0.0f, 1.2f, low); 
            }
        }

        if (vignette != null)
        {
            if (corduraActual >= umbral)
            {
                vignette.intensity.value = 0f;
            }
            else
            {
                vignette.active = true;
                vignette.intensity.value = Mathf.Lerp(0.0f, 0.55f, low);
                vignette.smoothness.value = Mathf.Lerp(0.25f, 0.6f, low);
                vignette.color.value = Color.Lerp(Color.black, new Color(0.6f, 0.05f, 0.05f, 1f), low);
            }
        }

    
        if (chroma != null)
        {
            chroma.active = true;
            chroma.intensity.value = Mathf.Lerp(0.0f, 0.35f, low); 
        }

        if (grain != null)
        {
            grain.active = true;
            grain.intensity.value = Mathf.Lerp(0.0f, 0.35f, low); 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LightZone>() != null)
            zonasDeLuzDentro++;

        SanityLight luz = other.GetComponentInParent<SanityLight>();
        if (luz != null)
        {
            luzActual = luz;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LightZone>() != null)
            zonasDeLuzDentro = Mathf.Max(0, zonasDeLuzDentro - 1);
        SanityLight luz = other.GetComponentInParent<SanityLight>();
        if (luz != null && luz == luzActual)
        {
            luzActual = null;
        }
    }
    public void RecuperarCordura(float cantidad)
    {
        cordura = Mathf.Clamp01(cordura + cantidad);
    }
}
