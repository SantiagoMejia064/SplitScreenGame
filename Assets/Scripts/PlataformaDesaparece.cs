using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlataformaDesaparece : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float tiempoAntesDeDesaparecer = 0.75f;
    [SerializeField] private float tiempoDesaparecida = 2f;

    [Header("Deteccion")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private bool soloSiCaeEncima = true;

    [Header("Partes")]
    [SerializeField] private Renderer[] renderers = null;
    [SerializeField] private Collider[] colliders = null;

    private bool desapareciendo;
    private bool[] renderersActivosIniciales;
    private bool[] collidersActivosIniciales;

    private void Awake()
    {
        BuscarPartesSiFaltan();
        GuardarEstadosIniciales();
    }

    private void Reset()
    {
        Collider plataformaCollider = GetComponent<Collider>();
        plataformaCollider.isTrigger = false;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        IntentarIniciar(collision.transform, collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        IntentarIniciar(other.transform, null);
    }

    private void IntentarIniciar(Transform target, Collision collision)
    {
        if (desapareciendo || Meta.juegoTerminado)
        {
            return;
        }

        if (collision != null && soloSiCaeEncima && !TieneContactoSuperior(collision))
        {
            return;
        }

        if (!EsJugador(target))
        {
            return;
        }

        StartCoroutine(DesaparecerTemporalmente());
    }

    private IEnumerator DesaparecerTemporalmente()
    {
        desapareciendo = true;

        if (tiempoAntesDeDesaparecer > 0f)
        {
            yield return new WaitForSeconds(tiempoAntesDeDesaparecer);
        }

        CambiarEstado(false);

        if (tiempoDesaparecida > 0f)
        {
            yield return new WaitForSeconds(tiempoDesaparecida);
        }

        RestaurarEstadoInicial();
        desapareciendo = false;
    }

    private void CambiarEstado(bool activo)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = activo;
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = activo;
            }
        }
    }

    private void RestaurarEstadoInicial()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = renderersActivosIniciales[i];
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = collidersActivosIniciales[i];
            }
        }
    }

    private void BuscarPartesSiFaltan()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        if (colliders == null || colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>();
        }
    }

    private void GuardarEstadosIniciales()
    {
        renderersActivosIniciales = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            renderersActivosIniciales[i] = renderers[i] != null && renderers[i].enabled;
        }

        collidersActivosIniciales = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            collidersActivosIniciales[i] = colliders[i] != null && colliders[i].enabled;
        }
    }

    private bool TieneContactoSuperior(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.45f)
            {
                return true;
            }
        }

        return false;
    }

    private bool EsJugador(Transform target)
    {
        Movimiento jugador = target.GetComponentInParent<Movimiento>();
        if (jugador != null)
        {
            return true;
        }

        Transform current = target;
        while (current != null)
        {
            if (!string.IsNullOrEmpty(tagJugador) && current.CompareTag(tagJugador))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}