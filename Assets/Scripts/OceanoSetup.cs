using UnityEngine;

public class OceanoSetup : MonoBehaviour
{
    private void Awake()
    {
        foreach (Transform hijo in transform)
        {
            Collider col = hijo.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning($"{hijo.name} no tiene Collider, se ignora.");
                continue;
            }

            col.isTrigger = true;

            if (hijo.GetComponent<Agua>() == null)
            {
                hijo.gameObject.AddComponent<Agua>();
            }
        }
    }
}
