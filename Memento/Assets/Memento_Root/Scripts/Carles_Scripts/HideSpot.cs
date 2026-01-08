using UnityEngine;

public class HideSpor : MonoBehaviour
{
    [Header("Puntos")]
    public Transform puntoEntrar;  
    public Transform puntoSalir;  

    [Header("Opciones")]
    public bool bloquearMovimiento = true;
    public bool ocultarJugadorVisual = false; 

    public string textoInteractuar = "Esconderse";
}
