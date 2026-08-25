using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubeYBaja : MonoBehaviour
{
    [Header("Pivote")]
    [SerializeField] private Transform puntoPivote = null;
    [SerializeField] private Vector3 ejeRotacionLocal = Vector3.forward;
    [SerializeField] private Vector3 ejeLadoLocal = Vector3.right;
    [SerializeField] private bool invertirDireccion = false;

    [Header("Movimiento")]
    [SerializeField] private float anguloMaximo = 25f;
    [SerializeField] private float sensibilidadPeso = 4f;
    [SerializeField] private float velocidadRotacion = 70f;
    [SerializeField] private float velocidadRetorno = 35f;
    [SerializeField] private bool usarMasaDelRigidbody = true;

    [Header("Deteccion")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private bool soloContactoSuperior = true;
    [SerializeField] private float tiempoParaOlvidarContacto = 0.12f;

    private readonly Dictionary<Rigidbody, float> jugadoresTocando = new Dictionary<Rigidbody, float>();
    private readonly List<Rigidbody> jugadoresAEliminar = new List<Rigidbody>();

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 posicionPivoteInicial;
    private float anguloActual;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        posicionPivoteInicial = puntoPivote != null ? puntoPivote.position : transform.position;
    }

    private void Reset()
    {
        Collider plataformaCollider = GetComponent<Collider>();
        plataformaCollider.isTrigger = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        RegistrarContacto(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        RegistrarContacto(collision);
    }

    private void FixedUpdate()
    {
        LimpiarContactosViejos();

        float pesoLateral = CalcularPesoLateral();
        if (invertirDireccion)
        {
            pesoLateral *= -1f;
        }

        float anguloObjetivo = Mathf.Clamp(pesoLateral * sensibilidadPeso, -anguloMaximo, anguloMaximo);
        float velocidad = jugadoresTocando.Count > 0 ? velocidadRotacion : velocidadRetorno;
        anguloActual = Mathf.MoveTowards(anguloActual, anguloObjetivo, velocidad * Time.fixedDeltaTime);

        AplicarRotacion();
    }

    private void RegistrarContacto(Collision collision)
    {
        if (Meta.juegoTerminado)
        {
            return;
        }

        if (soloContactoSuperior && !TieneContactoSuperior(collision))
        {
            return;
        }

        Rigidbody jugadorRb = ObtenerRigidbodyJugador(collision);
        if (jugadorRb == null)
        {
            return;
        }

        jugadoresTocando[jugadorRb] = Time.time;
    }

    private float CalcularPesoLateral()
    {
        float total = 0f;
        Vector3 ladoMundo = transform.TransformDirection(ejeLadoLocal.sqrMagnitude > 0.001f ? ejeLadoLocal.normalized : Vector3.right);

        foreach (KeyValuePair<Rigidbody, float> contacto in jugadoresTocando)
        {
            Rigidbody jugadorRb = contacto.Key;
            if (jugadorRb == null)
            {
                continue;
            }

            float distanciaAlLado = Vector3.Dot(jugadorRb.worldCenterOfMass - posicionPivoteInicial, ladoMundo);
            float peso = usarMasaDelRigidbody ? Mathf.Max(0.1f, jugadorRb.mass) : 1f;
            total += distanciaAlLado * peso;
        }

        return total;
    }

    private void AplicarRotacion()
    {
        Vector3 ejeMundo = rotacionInicial * (ejeRotacionLocal.sqrMagnitude > 0.001f ? ejeRotacionLocal.normalized : Vector3.forward);
        Quaternion rotacionPeso = Quaternion.AngleAxis(anguloActual, ejeMundo);
        Vector3 offsetInicial = posicionInicial - posicionPivoteInicial;

        transform.SetPositionAndRotation(posicionPivoteInicial + rotacionPeso * offsetInicial, rotacionPeso * rotacionInicial);
    }

    private Rigidbody ObtenerRigidbodyJugador(Collision collision)
    {
        Movimiento jugador = collision.transform.GetComponentInParent<Movimiento>();
        if (jugador != null)
        {
            return jugador.GetComponent<Rigidbody>();
        }

        Transform current = collision.transform;
        while (current != null)
        {
            if (!string.IsNullOrEmpty(tagJugador) && current.CompareTag(tagJugador))
            {
                Rigidbody rb = current.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    return rb;
                }
            }

            current = current.parent;
        }

        return collision.rigidbody;
    }

    private void LimpiarContactosViejos()
    {
        jugadoresAEliminar.Clear();

        foreach (KeyValuePair<Rigidbody, float> contacto in jugadoresTocando)
        {
            if (contacto.Key == null || Time.time > contacto.Value + tiempoParaOlvidarContacto)
            {
                jugadoresAEliminar.Add(contacto.Key);
            }
        }

        for (int i = 0; i < jugadoresAEliminar.Count; i++)
        {
            jugadoresTocando.Remove(jugadoresAEliminar[i]);
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
}