using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;

    [SerializeField] private Transform[] spawnPoints = null;
    [SerializeField] private int framesDeAseguramiento = 3;

    private bool subscribed;
    private readonly HashSet<PlayerInput> jugadoresPosicionados = new HashSet<PlayerInput>();
    private readonly Dictionary<GameObject, Transform> spawnDePlayer = new Dictionary<GameObject, Transform>();
    private readonly Dictionary<GameObject, int> progresoDePlayer = new Dictionary<GameObject, int>();
    private readonly Dictionary<PlayerInput, Coroutine> rutinasDeSpawn = new Dictionary<PlayerInput, Coroutine>();

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void Update()
    {
        if (!subscribed)
        {
            TrySubscribe();
        }
    }

    private void OnDisable()
    {
        if (subscribed && PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= HandlePlayerJoined;
        }

        subscribed = false;
        rutinasDeSpawn.Clear();
        jugadoresPosicionados.Clear();
    }

    private void TrySubscribe()
    {
        if (subscribed || PlayerInputManager.instance == null)
        {
            return;
        }

        PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
        subscribed = true;

        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            HandlePlayerJoined(playerInput);
        }
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null || jugadoresPosicionados.Contains(playerInput))
        {
            return;
        }

        Transform spawnPoint = GetSpawnPoint(playerInput);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"No hay spawn point valido para {playerInput.name} con index {playerInput.playerIndex}.");
            return;
        }

        if (rutinasDeSpawn.TryGetValue(playerInput, out Coroutine rutinaAnterior))
        {
            StopCoroutine(rutinaAnterior);
        }

        rutinasDeSpawn[playerInput] = StartCoroutine(PosicionarJugador(playerInput, spawnPoint));
    }

    private IEnumerator PosicionarJugador(PlayerInput playerInput, Transform spawnPoint)
    {
        GameObject player = playerInput.gameObject;
        spawnDePlayer[player] = spawnPoint;
        progresoDePlayer[player] = -1;

        Teletransportar(player, spawnPoint);

        for (int i = 0; i < framesDeAseguramiento; i++)
        {
            yield return null;
            Teletransportar(player, spawnPoint);
        }

        yield return new WaitForFixedUpdate();
        Teletransportar(player, spawnPoint);

        jugadoresPosicionados.Add(playerInput);
        rutinasDeSpawn.Remove(playerInput);
    }

    private Transform GetSpawnPoint(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;
        if (index < 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        if (index >= spawnPoints.Length)
        {
            index %= spawnPoints.Length;
        }

        return spawnPoints[index];
    }

    private void Teletransportar(GameObject player, Transform spawnPoint)
    {
        if (player == null || spawnPoint == null)
        {
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;
        }

        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        Physics.SyncTransforms();
    }

    public bool SetRespawnPoint(GameObject player, Transform spawnPoint)
    {
        return SetRespawnPoint(player, spawnPoint, 0, false);
    }

    public bool SetRespawnPoint(GameObject player, Transform spawnPoint, int indiceProgreso, bool soloSiAvanza)
    {
        if (player == null || spawnPoint == null)
        {
            return false;
        }

        GameObject rootPlayer = player;
        if (TryGetPlayerRoot(player, out GameObject jugadorEncontrado))
        {
            rootPlayer = jugadorEncontrado;
        }

        if (soloSiAvanza && progresoDePlayer.TryGetValue(rootPlayer, out int progresoActual) && indiceProgreso <= progresoActual)
        {
            return false;
        }

        spawnDePlayer[rootPlayer] = spawnPoint;
        progresoDePlayer[rootPlayer] = indiceProgreso;
        return true;
    }

    public bool TryGetPlayerRoot(GameObject candidate, out GameObject rootPlayer)
    {
        return TryGetPlayerRoot(candidate, "Player", out rootPlayer);
    }

    public bool TryGetPlayerRoot(GameObject candidate, string tagJugador, out GameObject rootPlayer)
    {
        rootPlayer = null;
        if (candidate == null)
        {
            return false;
        }

        Movimiento movimiento = candidate.GetComponentInParent<Movimiento>();
        if (movimiento != null)
        {
            rootPlayer = movimiento.gameObject;
            return true;
        }

        Transform current = candidate.transform;
        while (current != null)
        {
            if (string.IsNullOrEmpty(tagJugador) || current.CompareTag(tagJugador))
            {
                rootPlayer = current.gameObject;
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    public void RespawnPlayer(GameObject player)
    {
        GameObject rootPlayer = player;
        if (TryGetPlayerRoot(player, out GameObject jugadorEncontrado))
        {
            rootPlayer = jugadorEncontrado;
        }

        if (rootPlayer == null || !spawnDePlayer.TryGetValue(rootPlayer, out Transform spawnPoint))
        {
            string nombre = rootPlayer != null ? rootPlayer.name : "Jugador desconocido";
            Debug.LogWarning($"{nombre} no tiene spawn point registrado.");
            return;
        }

        Teletransportar(rootPlayer, spawnPoint);
    }
}