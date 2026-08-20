using UnityEngine;
using System.Collections.Generic;

public class Meta : MonoBehaviour
{
    public static bool juegoTerminado = false;

    private static List<GameObject> jugadoresQueLlegaron = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (juegoTerminado) return;
        if (!other.CompareTag("Player")) return;
        if (jugadoresQueLlegaron.Contains(other.gameObject)) return;

        jugadoresQueLlegaron.Add(other.gameObject);
        int puesto = jugadoresQueLlegaron.Count;

        Debug.Log($"{other.gameObject.name} llegó en el puesto {puesto}");

        if (puesto == 1)
        {
            juegoTerminado = true;
            Debug.Log($"¡{other.gameObject.name} GANÓ la carrera!");
        }
    }
}
