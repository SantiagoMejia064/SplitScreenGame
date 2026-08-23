using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private bool subscribed;
    private readonly HashSet<int> positionedPlayerIndexes = new HashSet<int>();

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
    }
}
