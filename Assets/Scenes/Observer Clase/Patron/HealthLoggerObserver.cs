using UnityEngine;

public class HealthLoggerObserver : MonoBehaviour, IHealthObserver
{
    [SerializeField] private Health health;

    private void OnEnable()
    {
        if (health != null)
        {
            health.AddObserver(this);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.RemoveObserver(this);
        }
    }

    public void OnHealthChanged(int currentHealth, int maximumHealth)
    {
        Debug.Log("HealthLoggerObserver: " + currentHealth + "/" + maximumHealth);
    }
}