using UnityEngine;

public class HealthLoggerBasic : MonoBehaviour
{
    public void LogHealth(int currentHealth, int maximumHealth)
    {
        Debug.Log("HealthLogger: " + currentHealth + "/" + maximumHealth);
    }
}
