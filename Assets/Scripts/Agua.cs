using UnityEngine;

public class Agua : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (Meta.juegoTerminado) return;

        if (other.CompareTag("Player"))
        {
            PlayerSpawnManager.instance.RespawnPlayer(other.gameObject);
        }
    }
}
