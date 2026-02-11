using UnityEngine;

public class SanityDrainEnemy : MonoBehaviour
{
    public float drenajePorSegundo = 0.08f; 
    public float suavizadoEntrada = 10f;

    float factor = 0f;
    bool dentro = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) dentro = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) dentro = false;
    }

    [System.Obsolete]
    void Update()
    {
        float objetivo = dentro ? 1f : 0f;
        factor = Mathf.Lerp(factor, objetivo, Time.deltaTime * suavizadoEntrada);

        if (factor > 0f)
        {
            var sanity = GameObject.FindObjectOfType<SanityController>();
            if (sanity != null)
                sanity.cordura = Mathf.Clamp01(sanity.cordura - drenajePorSegundo * factor * Time.deltaTime);
        }
    }
}
