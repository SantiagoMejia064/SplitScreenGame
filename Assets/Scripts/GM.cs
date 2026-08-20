using UnityEngine;
using TMPro;

public class GM : MonoBehaviour
{
    [Header("Textos de intro")]
    public GameObject textoJugador1;
    public GameObject textoJugador2;
    public float duracionIntro = 3f;

    [Header("Texto de ganador")]
    public GameObject textoGanador;
    public TextMeshProUGUI textoGanadorTMP; // el componente de texto dentro de textoGanador

    [Header("Jugadores (para congelarlos al ganar)")]
    public GameObject player1;
    public GameObject player2;

    private bool carreraTerminada = false;

    private void Start()
    {
        textoJugador1.SetActive(true);
        textoJugador2.SetActive(true);
        textoGanador.SetActive(false);

        Invoke(nameof(OcultarIntro), duracionIntro);
    }

    private void OcultarIntro()
    {
        textoJugador1.SetActive(false);
        textoJugador2.SetActive(false);
    }

    // ===== Llamada desde el script de la Meta cuando alguien llega =====
    public void DeclararGanador(int numeroJugador)
    {
        if (carreraTerminada) return; // evita que el segundo jugador también "gane"
        carreraTerminada = true;

        textoGanador.SetActive(true);
        textoGanadorTMP.text = $"¡El Jugador {numeroJugador} gana!";

        // Congela a ambos jugadores desactivando su script de movimiento
        player1.GetComponent<MovPlayer>().enabled = false;
        player2.GetComponent<MovPlayer>().enabled = false;
    }
}