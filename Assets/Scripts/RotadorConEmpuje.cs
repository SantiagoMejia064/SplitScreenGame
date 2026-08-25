using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RotadorConEmpuje : MonoBehaviour
{
    private enum TipoDireccionEmpuje
    {
        Tangencial,
        Vertical
    }

    [Header("Rotacion")]
    [SerializeField] private Transform puntoRotacion = null;
    [SerializeField] private Vector3 ejeRotacion = Vector3.up;
    [SerializeField] private float velocidadGradosPorSegundo = 90f;
    [SerializeField] private bool rotarAlrededorDelPunto = true;

    [Header("Swing")]
    [SerializeField] private bool usarSwing = false;
    [SerializeField] private float anguloMinimoSwing = -25f;
    [SerializeField] private float anguloMaximoSwing = 25f;

    [Header("Comportamiento")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private bool usarEmpuje = true;
    [SerializeField] private bool respawnearJugador = false;

    [Header("Empuje")]
    [SerializeField] private TipoDireccionEmpuje direccionEmpuje = TipoDireccionEmpuje.Tangencial;
    [SerializeField] private float fuerzaEmpuje = 12f;
    [SerializeField] private float fuerzaVertical = 1.5f;
    [SerializeField] private float intervaloEntreEmpujes = 0.15f;
    [SerializeField] private ForceMode modoEmpuje = ForceMode.Impulse;

    [Header("Resorte Visual")]
    [SerializeField] private bool usarAnimacionResorte = false;
    [SerializeField] private Transform modeloResorte = null;
    [SerializeField] private bool animarSoloSiCaeEncima = true;
    [SerializeField] private float alturaHundimiento = 0.25f;
    [SerializeField] private float alturaRebote = 0.15f;
    [SerializeField] private float duracionHundimiento = 0.08f;
    [SerializeField] private float duracionRebote = 0.12f;
    [SerializeField] private float duracionRetorno = 0.14f;
    [SerializeField] private float intervaloEntreAnimaciones = 0.2f;

    private readonly Dictionary<Rigidbody, float> ultimoEmpujePorJugador = new Dictionary<Rigidbody, float>();
    private readonly Dictionary<GameObject, float> ultimoRespawnPorJugador = new Dictionary<GameObject, float>();
    private const float intervaloEntreRespawns = 0.25f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 posicionInicialModelo;
    private Coroutine rutinaResorte;
    private float ultimoInicioResorte;
    private float tiempoSwing;
    private float ultimoAnguloSwing;
    private float signoMovimientoRotacion = 1f;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        if (modeloResorte != null)
        {
            posicionInicialModelo = modeloResorte.localPosition;
        }
    }

    private void Reset()
    {
        Collider obstacleCollider = GetComponent<Collider>();
        obstacleCollider.isTrigger = false;
    }

    private void FixedUpdate()
    {
        Vector3 axis = ejeRotacion.sqrMagnitude > 0.001f ? ejeRotacion.normalized : Vector3.up;

        if (usarSwing)
        {
            AplicarSwing(axis);
            return;
        }

        float degrees = velocidadGradosPorSegundo * Time.fixedDeltaTime;
        if (Mathf.Abs(degrees) > 0.001f)
        {
            signoMovimientoRotacion = Mathf.Sign(degrees);
        }

        if (puntoRotacion != null && rotarAlrededorDelPunto)
        {
            transform.RotateAround(puntoRotacion.position, axis, degrees);
            return;
        }

        transform.Rotate(axis, degrees, Space.Self);
    }

    private void AplicarSwing(Vector3 axis)
    {
        float minimo = Mathf.Min(anguloMinimoSwing, anguloMaximoSwing);
        float maximo = Mathf.Max(anguloMinimoSwing, anguloMaximoSwing);
        float centro = (minimo + maximo) * 0.5f;
        float amplitud = (maximo - minimo) * 0.5f;

        if (amplitud <= 0.001f)
        {
            return;
        }

        tiempoSwing += Time.fixedDeltaTime;
        float velocidad = Mathf.Abs(velocidadGradosPorSegundo);
        float frecuencia = velocidad > 0.001f ? velocidad / amplitud : 0f;
        float angulo = centro + Mathf.Sin(tiempoSwing * frecuencia) * amplitud;
        float deltaAngulo = angulo - ultimoAnguloSwing;

        if (Mathf.Abs(deltaAngulo) > 0.001f)
        {
            signoMovimientoRotacion = Mathf.Sign(deltaAngulo);
        }

        ultimoAnguloSwing = angulo;

        if (puntoRotacion != null && rotarAlrededorDelPunto)
        {
            Quaternion rotacionSwing = Quaternion.AngleAxis(angulo, axis);
            Vector3 offsetInicial = posicionInicial - puntoRotacion.position;
            transform.SetPositionAndRotation(puntoRotacion.position + rotacionSwing * offsetInicial, rotacionSwing * rotacionInicial);
            return;
        }

        transform.SetPositionAndRotation(posicionInicial, rotacionInicial * Quaternion.AngleAxis(angulo, axis));
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcesarJugador(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProcesarJugador(collision);
    }

    private void ProcesarJugador(Collision collision)
    {
        GameObject jugador = ObtenerJugador(collision.transform);
        if (jugador == null)
        {
            return;
        }

        IntentarAnimarResorte(collision);

        if (respawnearJugador)
        {
            Respawnear(jugador);
        }

        if (usarEmpuje)
        {
            EmpujarJugador(collision, jugador);
        }
    }

    private void EmpujarJugador(Collision collision, GameObject jugador)
    {
        Rigidbody jugadorRb = collision.rigidbody != null ? collision.rigidbody : jugador.GetComponent<Rigidbody>();
        if (jugadorRb == null || !PuedeEmpujar(jugadorRb))
        {
            return;
        }

        Vector3 direccion = CalcularDireccionFinalEmpuje(collision);
        jugadorRb.AddForce(direccion * fuerzaEmpuje, modoEmpuje);
        ultimoEmpujePorJugador[jugadorRb] = Time.time;
    }

    private Vector3 CalcularDireccionFinalEmpuje(Collision collision)
    {
        if (direccionEmpuje == TipoDireccionEmpuje.Vertical)
        {
            return Vector3.up;
        }

        Vector3 direccion = CalcularDireccionEmpuje(collision);
        direccion.y += fuerzaVertical;
        return direccion.normalized;
    }

    private void Respawnear(GameObject jugador)
    {
        if (PlayerSpawnManager.instance == null || !PuedeRespawnear(jugador))
        {
            return;
        }

        PlayerSpawnManager.instance.RespawnPlayer(jugador);
        ultimoRespawnPorJugador[jugador] = Time.time;
    }

    private void IntentarAnimarResorte(Collision collision)
    {
        if (!usarAnimacionResorte || modeloResorte == null || Time.time < ultimoInicioResorte + intervaloEntreAnimaciones)
        {
            return;
        }

        if (animarSoloSiCaeEncima && !TieneContactoSuperior(collision))
        {
            return;
        }

        if (rutinaResorte != null)
        {
            StopCoroutine(rutinaResorte);
        }

        ultimoInicioResorte = Time.time;
        rutinaResorte = StartCoroutine(AnimarResorte());
    }

    private IEnumerator AnimarResorte()
    {
        Vector3 abajo = posicionInicialModelo + Vector3.down * alturaHundimiento;
        Vector3 arriba = posicionInicialModelo + Vector3.up * alturaRebote;

        yield return MoverModeloResorte(modeloResorte.localPosition, abajo, duracionHundimiento);
        yield return MoverModeloResorte(abajo, arriba, duracionRebote);
        yield return MoverModeloResorte(arriba, posicionInicialModelo, duracionRetorno);

        modeloResorte.localPosition = posicionInicialModelo;
        rutinaResorte = null;
    }

    private IEnumerator MoverModeloResorte(Vector3 origen, Vector3 destino, float duracion)
    {
        if (duracion <= 0f)
        {
            modeloResorte.localPosition = destino;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duracion)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);
            modeloResorte.localPosition = Vector3.Lerp(origen, destino, t);
            yield return null;
        }

        modeloResorte.localPosition = destino;
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

    private GameObject ObtenerJugador(Transform target)
    {
        Movimiento jugador = target.GetComponentInParent<Movimiento>();
        if (jugador != null)
        {
            return jugador.gameObject;
        }

        Transform current = target;
        while (current != null)
        {
            if (current.CompareTag(tagJugador))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    private bool PuedeEmpujar(Rigidbody jugadorRb)
    {
        return !ultimoEmpujePorJugador.TryGetValue(jugadorRb, out float ultimoEmpuje)
            || Time.time >= ultimoEmpuje + intervaloEntreEmpujes;
    }

    private bool PuedeRespawnear(GameObject jugador)
    {
        return !ultimoRespawnPorJugador.TryGetValue(jugador, out float ultimoRespawn)
            || Time.time >= ultimoRespawn + intervaloEntreRespawns;
    }

    private Vector3 CalcularDireccionEmpuje(Collision collision)
    {
        Vector3 center = puntoRotacion != null ? puntoRotacion.position : transform.position;
        Vector3 axis = ObtenerEjeMundo();

        if (collision.contactCount > 0)
        {
            Vector3 radio = collision.GetContact(0).point - center;
            Vector3 tangente = Vector3.Cross(axis, radio).normalized * signoMovimientoRotacion;

            if (tangente.sqrMagnitude > 0.001f)
            {
                return tangente;
            }
        }

        Vector3 direction = collision.transform.position - center;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
    }

    private Vector3 ObtenerEjeMundo()
    {
        Vector3 axis = ejeRotacion.sqrMagnitude > 0.001f ? ejeRotacion.normalized : Vector3.up;
        return puntoRotacion != null && rotarAlrededorDelPunto ? axis : transform.TransformDirection(axis);
    }
}