using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrampaPinchosCaen : MonoBehaviour
{
    [Header("Pinchos")]
    [SerializeField] private Rigidbody[] pinchos = null;
    [SerializeField] private bool buscarPinchosEnHijos = true;
    [SerializeField] private bool mantenerSuspendidosAlInicio = true;
    [SerializeField] private Vector3 velocidadInicialCaida = Vector3.zero;

    [Header("Tiempos")]
    [SerializeField] private float tiempoAntesDeCaer = 0.35f;
    [SerializeField] private bool reiniciarAutomaticamente = true;
    [SerializeField] private float tiempoAntesDeReiniciar = 3f;
    [SerializeField] private bool activarUnaSolaVez = false;

    [Header("Deteccion")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private bool mostrarLogs = false;

    private Vector3[] posicionesIniciales;
    private Quaternion[] rotacionesIniciales;
    private bool[] useGravityInicial;
    private bool[] isKinematicInicial;
    private bool activada;
    private bool usada;
    private Coroutine rutinaTrampa;

    private void Awake()
    {
        BuscarPinchosSiFaltan();
        GuardarEstadoInicial();
        PrepararPinchos();
    }

    private void Reset()
    {
        Collider area = GetComponent<Collider>();
        area.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        IntentarActivar(other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        IntentarActivar(collision.transform);
    }

    private void IntentarActivar(Transform target)
    {
        if (Meta.juegoTerminado || activada || (activarUnaSolaVez && usada))
        {
            return;
        }

        if (!EsJugador(target))
        {
            return;
        }

        rutinaTrampa = StartCoroutine(ActivarTrampa());
    }

    private IEnumerator ActivarTrampa()
    {
        activada = true;
        usada = true;

        if (mostrarLogs)
        {
            Debug.Log($"Trampa de pinchos activada: {name}");
        }

        if (tiempoAntesDeCaer > 0f)
        {
            yield return new WaitForSeconds(tiempoAntesDeCaer);
        }

        SoltarPinchos();

        if (reiniciarAutomaticamente && !activarUnaSolaVez)
        {
            if (tiempoAntesDeReiniciar > 0f)
            {
                yield return new WaitForSeconds(tiempoAntesDeReiniciar);
            }

            ReiniciarPinchos();
            activada = false;
            rutinaTrampa = null;
            yield break;
        }

        rutinaTrampa = null;
    }

    private void SoltarPinchos()
    {
        for (int i = 0; i < pinchos.Length; i++)
        {
            Rigidbody pincho = pinchos[i];
            if (pincho == null)
            {
                continue;
            }

            pincho.isKinematic = false;
            pincho.useGravity = true;
            pincho.linearVelocity = velocidadInicialCaida;
            pincho.angularVelocity = Vector3.zero;
        }
    }

    private void ReiniciarPinchos()
    {
        for (int i = 0; i < pinchos.Length; i++)
        {
            Rigidbody pincho = pinchos[i];
            if (pincho == null)
            {
                continue;
            }

            pincho.linearVelocity = Vector3.zero;
            pincho.angularVelocity = Vector3.zero;
            pincho.isKinematic = true;
            pincho.useGravity = false;
            pincho.transform.SetPositionAndRotation(posicionesIniciales[i], rotacionesIniciales[i]);
        }
    }

    private void PrepararPinchos()
    {
        for (int i = 0; i < pinchos.Length; i++)
        {
            Rigidbody pincho = pinchos[i];
            if (pincho == null)
            {
                continue;
            }

            PinchoRespawn detector = pincho.GetComponent<PinchoRespawn>();
            if (detector == null)
            {
                detector = pincho.gameObject.AddComponent<PinchoRespawn>();
            }

            detector.Configurar(tagJugador);

            if (mantenerSuspendidosAlInicio)
            {
                pincho.linearVelocity = Vector3.zero;
                pincho.angularVelocity = Vector3.zero;
                pincho.useGravity = false;
                pincho.isKinematic = true;
            }
        }
    }

    private void BuscarPinchosSiFaltan()
    {
        if ((pinchos == null || pinchos.Length == 0) && buscarPinchosEnHijos)
        {
            pinchos = GetComponentsInChildren<Rigidbody>();
        }

        if (pinchos == null)
        {
            pinchos = new Rigidbody[0];
        }
    }

    private void GuardarEstadoInicial()
    {
        posicionesIniciales = new Vector3[pinchos.Length];
        rotacionesIniciales = new Quaternion[pinchos.Length];
        useGravityInicial = new bool[pinchos.Length];
        isKinematicInicial = new bool[pinchos.Length];

        for (int i = 0; i < pinchos.Length; i++)
        {
            Rigidbody pincho = pinchos[i];
            if (pincho == null)
            {
                continue;
            }

            posicionesIniciales[i] = pincho.transform.position;
            rotacionesIniciales[i] = pincho.transform.rotation;
            useGravityInicial[i] = pincho.useGravity;
            isKinematicInicial[i] = pincho.isKinematic;
        }
    }

    private void OnDisable()
    {
        if (rutinaTrampa != null)
        {
            StopCoroutine(rutinaTrampa);
            rutinaTrampa = null;
        }

        RestaurarEstadoFisicoInicial();
    }

    private void RestaurarEstadoFisicoInicial()
    {
        if (pinchos == null || posicionesIniciales == null)
        {
            return;
        }

        for (int i = 0; i < pinchos.Length; i++)
        {
            Rigidbody pincho = pinchos[i];
            if (pincho == null)
            {
                continue;
            }

            pincho.linearVelocity = Vector3.zero;
            pincho.angularVelocity = Vector3.zero;
            pincho.useGravity = useGravityInicial[i];
            pincho.isKinematic = isKinematicInicial[i];
            pincho.transform.SetPositionAndRotation(posicionesIniciales[i], rotacionesIniciales[i]);
        }
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