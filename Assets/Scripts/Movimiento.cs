using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    private Vector2 moveInput;
    [SerializeField] private float speed = 5f;

    private float fuerzaSalto = 5f;
    private Rigidbody rb;

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
        Debug.Log("Saltar");
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
    }

    private void Update()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
