using UnityEngine;
using UnityEngine.InputSystem;

public class MovPlayer : MonoBehaviour
{
    public enum TipoControl { Teclado, Control }

    [Header("Asignación de dispositivo")]
    public TipoControl tipoControl;

    private NIS inputActions;
    private Vector2 moveInput;
    public float speed = 5f;
    private float fuerzaSalto = 5f;
    private Rigidbody rb;

    [Header("Detección de suelo")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private MovilPlatform plataformaActual;

    private void Awake()
    {
        inputActions = new NIS();
        rb = GetComponent<Rigidbody>();

        if (tipoControl == TipoControl.Teclado)
        {
            inputActions.devices = new InputDevice[] { Keyboard.current };
        }
        else
        {
            inputActions.devices = new InputDevice[] { Gamepad.current };
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            Debug.Log("Saltar");
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * speed * Time.fixedDeltaTime;
        Vector3 movimientoPlataforma = plataformaActual != null ? plataformaActual.DeltaMovimiento : Vector3.zero;

        rb.MovePosition(rb.position + movement + movimientoPlataforma);

        // ===== CAMBIO 1: SphereCast en vez de Raycast, más tolerante con el borde de la cápsula =====
        isGrounded = Physics.SphereCast(rb.position, 0.3f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            // ===== CAMBIO 2: GetComponentInParent en vez de TryGetComponent =====
            // Por si el Collider de la plataforma está en un hijo distinto al script MovilPlatform
            plataformaActual = hit.collider.GetComponentInParent<MovilPlatform>();

            // ===== CAMBIO 3: Debug temporal para verificar que sí está detectando la plataforma =====
            // Bórralo cuando confirmes que funciona
            Debug.Log(plataformaActual != null ? $"Sobre plataforma: {plataformaActual.name}" : "Suelo normal (no es plataforma)");
        }
        else
        {
            plataformaActual = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}