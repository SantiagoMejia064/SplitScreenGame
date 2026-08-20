using UnityEngine;

public class Meta : MonoBehaviour
{
    [Header("Número de este jugador (1 o 2)")]
    public int numeroJugador;

    private GM gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GM>(); // Unity 6: reemplaza a FindObjectOfType
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica que quien entró tiene el script Movimiento (o sea, es un jugador)
        if (other.TryGetComponent<MovPlayer>(out MovPlayer jugador))
        {
            gameManager.DeclararGanador(numeroJugador);
        }
    }
}