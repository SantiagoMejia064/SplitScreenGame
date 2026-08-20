using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    private Vector2 moveInput;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float fuerzaSalto = 5f;

    private Rigidbody rb;
    private int groundContacts = 0;
    private bool isGrounded => groundContacts > 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    private void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    private void OnJump(InputValue input)
    {
        if (Meta.juegoTerminado) return;

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        if (Meta.juegoTerminado)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            groundContacts++;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            groundContacts--;
    }
}