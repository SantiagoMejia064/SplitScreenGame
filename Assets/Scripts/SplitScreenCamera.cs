using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Camera))]
public class SplitScreenCamera : MonoBehaviour
{
    private Camera cam;
    private Canvas playerCanvas;
    private TextMeshProUGUI playerLabel;
    private int index;
    private int totalPlayers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        cam = GetComponent<Camera>();

        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null)
        {
            index = playerInput.playerIndex;
            playerCanvas = playerInput.GetComponentInChildren<Canvas>(true);
            playerLabel = FindPlayerLabel(playerInput);
        }

        SetupPlayerCanvas();
        UpdatePlayerLabel();

        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
        }
    }

    void Start()
    {
        index = GetComponentInParent<PlayerInput>().playerIndex;
        totalPlayers = PlayerInput.all.Count;
        cam = GetComponent<Camera>();
        cam.depth = index;

        SetupCamera();
        SetupPlayerCanvas();
        UpdatePlayerLabel();
    }

    private void OnDestroy()
    {
        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= HandlePlayerJoined;
        }
    }

    private void HandlePlayerJoined(PlayerInput obj)
    {
        totalPlayers = PlayerInput.all.Count;
        SetupCamera();
        SetupPlayerCanvas();
        UpdatePlayerLabel();
    }

    private void SetupCamera()
    {
        if (totalPlayers == 1)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (totalPlayers == 2)
        {
            cam.rect = new Rect(index == 0 ? 0 : 0.5f, 0, 0.5f, 1);
        }
        else if (totalPlayers == 3)
        {
            cam.rect = new Rect(
                index == 0 ? 0 : (index == 1 ? 0.5f : 0),
                index < 2 ? 0.5f : 0,
                index < 2 ? 0.5f : 1,
                0.5f);
        }
        else
        {
            cam.rect = new Rect((index % 2) * 0.5f, (index < 2) ? 0.5f : 0f, 0.5f, 0.5f);
        }

    }

    private void SetupPlayerCanvas()
    {
        if (playerCanvas == null)
        {
            return;
        }

        playerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        playerCanvas.worldCamera = cam;
        playerCanvas.planeDistance = cam.nearClipPlane + 0.1f;
        playerCanvas.overrideSorting = true;
        playerCanvas.sortingOrder = 100;
    }

    private void UpdatePlayerLabel()
    {
        if (playerLabel == null)
        {
            return;
        }

        playerLabel.text = $"JUGADOR {index + 1}";
    }

    private TextMeshProUGUI FindPlayerLabel(PlayerInput playerInput)
    {
        TextMeshProUGUI[] labels = playerInput.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI label in labels)
        {
            if (label.gameObject.name == "Num Jugador")
            {
                return label;
            }
        }

        foreach (TextMeshProUGUI label in labels)
        {
            if (label.gameObject.name != "StartText")
            {
                return label;
            }
        }

        return null;
    }
}
