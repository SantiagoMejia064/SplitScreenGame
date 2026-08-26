using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaceTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime;
    private bool timerRunning;

    private void Awake()
    {
        FindTimerTextIfNeeded();
        UpdateTimerText();
    }

    private void OnEnable()
    {
        Movimiento.RaceStarted += HandleRaceStarted;

        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
        }
    }

    private void Start()
    {
        if (Movimiento.carreraIniciada)
        {
            StartTimer();
        }
    }

    private void OnDisable()
    {
        Movimiento.RaceStarted -= HandleRaceStarted;

        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= HandlePlayerJoined;
        }
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        if (Meta.juegoTerminado)
        {
            timerRunning = false;
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        UpdateTimerText();
    }

    private void HandleRaceStarted()
    {
        StartTimer();
    }

    private void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    private void FindTimerTextIfNeeded()
    {
        if (timerText != null)
        {
            return;
        }

        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name.ToLower() == "timer")
            {
                timerText = text;
                return;
            }
        }
    }
}
