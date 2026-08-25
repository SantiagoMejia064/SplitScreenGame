using System.Collections.Generic;
using UnityEngine;

public class PinchoRespawn : MonoBehaviour
{
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private float intervaloEntreRespawns = 0.25f;

    private readonly Dictionary<GameObject, float> ultimoRespawnPorJugador = new Dictionary<GameObject, float>();

    public void Configurar(string nuevoTagJugador)
    {
        tagJugador = nuevoTagJugador;
    }

    private void OnCollisionEnter(Collision collision)
    {
        IntentarRespawn(collision.transform);
    }

    private void OnCollisionStay(Collision collision)
    {
        IntentarRespawn(collision.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        IntentarRespawn(other.transform);
    }

    private void IntentarRespawn(Transform target)
    {
        if (Meta.juegoTerminado || PlayerSpawnManager.instance == null)
        {
            return;
        }

        GameObject jugador = ObtenerJugador(target);
        if (jugador == null || !PuedeRespawnear(jugador))
        {
            return;
        }

        PlayerSpawnManager.instance.RespawnPlayer(jugador);
        ultimoRespawnPorJugador[jugador] = Time.time;
    }

    private GameObject ObtenerJugador(Transform target)
    {
        Movimiento movimiento = target.GetComponentInParent<Movimiento>();
        if (movimiento != null)
        {
            return movimiento.gameObject;
        }

        Transform current = target;
        while (current != null)
        {
            if (!string.IsNullOrEmpty(tagJugador) && current.CompareTag(tagJugador))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    private bool PuedeRespawnear(GameObject jugador)
    {
        return !ultimoRespawnPorJugador.TryGetValue(jugador, out float ultimoRespawn)
            || Time.time >= ultimoRespawn + intervaloEntreRespawns;
    }
}