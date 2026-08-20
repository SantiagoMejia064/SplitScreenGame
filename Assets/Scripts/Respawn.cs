using UnityEngine;


public class Respawn : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint; 

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void RespawnJugador()
    {
        transform.position = respawnPoint.position;
        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero; // por si la cápsula quedó rotando
    }
}