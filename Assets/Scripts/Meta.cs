using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Meta : MonoBehaviour
{
    public static bool juegoTerminado = false;

    [SerializeField] private TextMeshProUGUI textoGanador;

    private static readonly List<GameObject> jugadoresQueLlegaron = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetMetaState()
    {
        juegoTerminado = false;
        jugadoresQueLlegaron.Clear();
    }

    private void Awake()
    {
        FindTextoGanadorIfNeeded();
        if (textoGanador != null)
        {
            textoGanador.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (juegoTerminado) return;
        if (!other.CompareTag("Player")) return;
        if (jugadoresQueLlegaron.Contains(other.gameObject)) return;

        jugadoresQueLlegaron.Add(other.gameObject);
        int puesto = jugadoresQueLlegaron.Count;

        if (puesto == 1)
        {
            juegoTerminado = true;
            MostrarGanador(other.gameObject);
        }
    }

    private void MostrarGanador(GameObject jugador)
    {
        if (textoGanador == null) return;

        textoGanador.gameObject.SetActive(true);
        textoGanador.text = $"{jugador.name} GANÓ!";
    }

    private void FindTextoGanadorIfNeeded()
    {
        if (textoGanador != null) return;

        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "TextoGanador")
            {
                textoGanador = text;
                return;
            }
        }
    }
}
