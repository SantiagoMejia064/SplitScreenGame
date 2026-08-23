using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;

    [SerializeField] private Transform[] spawnPoints;
    private bool subscribed;
    private readonly HashSet<int> positionedPlayerIndexes = new HashSet<int>();
    private readonly Dictionary<GameObject, Transform> spawnDePlayer = new Dictionary<GameObject, Transform>();

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
        int index = playerInput.playerIndex;
        if (positionedPlayerIndexes.Contains(index))
        {
            return;
        }
        if (index < 0 || index >= spawnPoints.Length || spawnPoints[index] == null)
        {
            return;
        }
        Transform spawnPoint = spawnPoints[index];
        playerInput.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        positionedPlayerIndexes.Add(index);

        spawnDePlayer[playerInput.gameObject] = spawnPoint;
    }

    public void RespawnPlayer(GameObject player)
    {
        if (!spawnDePlayer.TryGetValue(player, out Transform spawnPoint))
        {
            Debug.LogWarning($"{player.name} no tiene spawn point registrado.");
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
}