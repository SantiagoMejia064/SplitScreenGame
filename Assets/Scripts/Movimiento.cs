using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Movimiento : MonoBehaviour
{
    public static event Action RaceStarted;
    public static bool carreraIniciada { get; private set; }

    private static readonly HashSet<Movimiento> jugadores = new HashSet<Movimiento>();
    private static readonly HashSet<Movimiento> jugadoresListos = new HashSet<Movimiento>();

    private Vector2 moveInput;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float aceleracion = 32f;
    [SerializeField] private float desaceleracion = 24f;
    [SerializeField] private float velocidadGiro = 9f;
    [SerializeField] private float controlEnAire = 0.45f;
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private float gravedadExtra = 18f;
    [SerializeField] private float fuerzaEmpuje = 12f;
    [SerializeField] private float rangoAgarre = 1.35f;
    [SerializeField] private float fuerzaAgarre = 22f;
    [SerializeField, Range(0.1f, 1f)] private float multiplicadorVelocidadAgarrando = 0.65f;
    [SerializeField] private Transform referenciaMovimiento;
    [SerializeField] private bool rotarJugadorConMovimiento = false;
    [SerializeField] private TextMeshProUGUI startText;

    private Rigidbody rb;
    private Rigidbody rigidbodyAgarrado;
    private PlayerInput playerInput;
    private InputAction grabAction;
    private float readyInputDelay;
    private int groundContacts = 0;
    private bool grabInput;
    private bool isGrounded => groundContacts > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRaceState()
    {
        carreraIniciada = false;
        jugadores.Clear();
        jugadoresListos.Clear();
        RaceStarted = null;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        grabAction = playerInput.actions["Player/Grab"];
        readyInputDelay = Time.time + 0.25f;
        ConfigureRigidbodyRotation();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        FindMovementReferenceIfNeeded();
        FindStartTextIfNeeded();
    }

    void Start()
    {
        GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        UpdateStartText();
    }

    private void OnEnable()
    {
        jugadores.Add(this);
        UpdateStartText();
    }

    private void OnDisable()
    {
        jugadores.Remove(this);
        jugadoresListos.Remove(this);
    }

    private void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();

        if (!carreraIniciada && !IsGamepadPlayer() && moveInput.y > 0.5f)
        {
            MarkReady();
        }
    }

    private void OnJump(InputValue input)
    {
        if (!carreraIniciada)
        {
            if (IsGamepadPlayer() && input.isPressed)
            {
                MarkReady();
            }

            return;
        }

        if (!input.isPressed || Meta.juegoTerminado) return;

        if (isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, 0f);
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if (!carreraIniciada || Meta.juegoTerminado)
        {
            StopRigidbody();
            return;
        }

        grabInput = grabAction != null && grabAction.IsPressed();

        ApplyHeavyMovement();
        ApplyExtraGravity();
        ApplyGrab();
    }

    private void ApplyHeavyMovement()
    {
        Vector3 inputDirection = GetMovementDirection();

        float speedMultiplier = rigidbodyAgarrado != null ? multiplicadorVelocidadAgarrando : 1f;
        Vector3 desiredVelocity = inputDirection * speed * speedMultiplier;
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float response = inputDirection.sqrMagnitude > 0.01f ? aceleracion : desaceleracion;
        float control = isGrounded ? 1f : controlEnAire;

        Vector3 velocityChange = desiredVelocity - currentHorizontalVelocity;
        velocityChange = Vector3.ClampMagnitude(velocityChange, response * control * Time.fixedDeltaTime);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        RotatePlayerIfNeeded(inputDirection);
    }

    private void RotatePlayerIfNeeded(Vector3 inputDirection)
    {
        if (!rotarJugadorConMovimiento || inputDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
        Quaternion nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, velocidadGiro * Time.fixedDeltaTime);
        rb.MoveRotation(nextRotation);
    }

    private void ConfigureRigidbodyRotation()
    {
        if (rotarJugadorConMovimiento)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
            return;
        }

        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.down * gravedadExtra, ForceMode.Acceleration);
        }
    }

    private void ApplyGrab()
    {
        if (!grabInput)
        {
            rigidbodyAgarrado = null;
            return;
        }

        if (rigidbodyAgarrado == null)
        {
            TryGrabPlayer();
        }

        if (rigidbodyAgarrado == null)
        {
            return;
        }

        Vector3 pullDirection = rb.position - rigidbodyAgarrado.position;
        pullDirection.y = 0f;

        rigidbodyAgarrado.AddForce(pullDirection * fuerzaAgarre, ForceMode.Acceleration);
        rb.AddForce(-pullDirection * (fuerzaAgarre * 0.35f), ForceMode.Acceleration);
    }

    private void TryGrabPlayer()
    {
        Vector3 grabCenter = rb.position + Vector3.up * 0.8f;
        Collider[] hits = Physics.OverlapSphere(grabCenter, rangoAgarre, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            Movimiento otherPlayer = hit.GetComponentInParent<Movimiento>();
            if (otherPlayer == null || otherPlayer == this)
            {
                continue;
            }

            rigidbodyAgarrado = otherPlayer.GetComponent<Rigidbody>();
            return;
        }
    }

    private void StopRigidbody()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rigidbodyAgarrado = null;
        grabInput = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGround(collision.transform))
        {
            groundContacts++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGround(collision.transform))
        {
            groundContacts = Mathf.Max(0, groundContacts - 1);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!carreraIniciada || Meta.juegoTerminado || moveInput.sqrMagnitude < 0.1f)
        {
            return;
        }

        Movimiento otherPlayer = collision.gameObject.GetComponentInParent<Movimiento>();
        if (otherPlayer == null || otherPlayer == this || collision.rigidbody == null)
        {
            return;
        }

        Vector3 pushDirection = GetMovementDirection();

        collision.rigidbody.AddForce(pushDirection * fuerzaEmpuje, ForceMode.Acceleration);
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        if (referenciaMovimiento == null)
        {
            return input;
        }

        Vector3 forward = referenciaMovimiento.forward;
        Vector3 right = referenciaMovimiento.right;
        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude < 0.001f || right.sqrMagnitude < 0.001f)
        {
            return input;
        }

        forward.Normalize();
        right.Normalize();

        Vector3 direction = right * input.x + forward * input.z;
        return direction.sqrMagnitude > 1f ? direction.normalized : direction;
    }

    private void FindMovementReferenceIfNeeded()
    {
        if (referenciaMovimiento != null)
        {
            return;
        }

        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            referenciaMovimiento = playerCamera.transform;
        }
    }

    private void MarkReady()
    {
        if (Time.time < readyInputDelay || jugadoresListos.Contains(this))
        {
            return;
        }

        jugadoresListos.Add(this);
        UpdateStartText();
        TryStartRace();
    }

    private static void TryStartRace()
    {
        if (carreraIniciada || jugadores.Count == 0 || jugadoresListos.Count < jugadores.Count)
        {
            return;
        }

        carreraIniciada = true;

        foreach (Movimiento jugador in jugadores)
        {
            jugador.HideStartText();
        }

        RaceStarted?.Invoke();
    }

    private void UpdateStartText()
    {
        if (startText == null)
        {
            return;
        }

        startText.gameObject.SetActive(!carreraIniciada);
        startText.text = jugadoresListos.Contains(this) ? "LISTO" : GetStartPrompt();
    }

    private string GetStartPrompt()
    {
        return IsGamepadPlayer() ? "PRESIONA X" : "PRESIONA W";
    }

    private bool IsGamepadPlayer()
    {
        return playerInput != null && playerInput.devices.Any(device => device is Gamepad);
    }

    private void HideStartText()
    {
        if (startText != null)
        {
            startText.gameObject.SetActive(false);
        }
    }

    private void FindStartTextIfNeeded()
    {
        if (startText != null)
        {
            return;
        }

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "StartText")
            {
                startText = text;
                return;
            }
        }
    }

    private bool IsGround(Transform target)
    {
        while (target != null)
        {
            if (target.CompareTag("Ground"))
            {
                return true;
            }

            target = target.parent;
        }

        return false;
    }
}