using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class Movimiento : MonoBehaviour
{
    public static event Action RaceStarted;
    public static bool carreraIniciada { get; private set; }

    private static readonly HashSet<Movimiento> jugadores = new HashSet<Movimiento>();
    private static readonly HashSet<Movimiento> jugadoresListos = new HashSet<Movimiento>();

    private Vector2 moveInput;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private TextMeshProUGUI startText;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private float readyInputDelay;
    private int groundContacts = 0;
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
        readyInputDelay = Time.time + 0.25f;
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

        if (Meta.juegoTerminado) return;

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        if (!carreraIniciada || Meta.juegoTerminado)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * speed * Time.deltaTime);
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
