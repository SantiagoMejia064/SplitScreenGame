using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private void OnEnable()
    {
        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
        }
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= HandlePlayerJoined;
        }
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;

        if (index < 0 || index >= spawnPoints.Length || spawnPoints[index] == null)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[index];
        playerInput.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
}
