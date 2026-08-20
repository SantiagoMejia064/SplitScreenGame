using UnityEngine;

public class ZonaCaida : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.TryGetComponent<Respawn>(out Respawn jugador))
        {
            jugador.RespawnJugador();
        }
    }
}