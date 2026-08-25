using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuntoRespawn : MonoBehaviour
{
    [SerializeField] private Transform puntoRespawn = null;
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private bool soloSiAvanza = false;
    [SerializeField] private int indiceProgreso = 0;
    [SerializeField] private ParticleSystem particulasActivacion = null;
    [SerializeField] private bool reproducirParticulasSoloSiActualiza = true;
    [SerializeField] private bool reiniciarParticulasAlActivar = true;
    [SerializeField] private bool mostrarLogs = false;

    private Transform SpawnTransform => puntoRespawn != null ? puntoRespawn : transform;

    private void Reset()
    {
        Collider area = GetComponent<Collider>();
        area.isTrigger = true;
        particulasActivacion = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Meta.juegoTerminado)
        {
            return;
        }

        if (PlayerSpawnManager.instance == null)
        {
            Debug.LogWarning("No hay PlayerSpawnManager en la escena.");
            return;
        }

        if (!PlayerSpawnManager.instance.TryGetPlayerRoot(other.gameObject, tagJugador, out GameObject jugador))
        {
            return;
        }

        bool actualizado = PlayerSpawnManager.instance.SetRespawnPoint(jugador, SpawnTransform, indiceProgreso, soloSiAvanza);
        if (actualizado && mostrarLogs)
        {
            Debug.Log($"{jugador.name} actualizo su respawn a {SpawnTransform.name}.");
        }

        if (actualizado || !reproducirParticulasSoloSiActualiza)
        {
            ReproducirParticulas();
        }
    }

    private void ReproducirParticulas()
    {
        if (particulasActivacion == null)
        {
            return;
        }

        if (reiniciarParticulasAlActivar)
        {
            particulasActivacion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        particulasActivacion.Play(true);
    }
}