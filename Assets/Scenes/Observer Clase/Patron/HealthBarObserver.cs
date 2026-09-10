using UnityEngine;
using UnityEngine.UI;

public class HealthBarObserver : MonoBehaviour, IHealthObserver
{
    [SerializeField] private Health health;
    [SerializeField] private Slider healthSlider;

    private void OnEnable()
    {
        if(health != null)
        {
            health.AddObserver(this);
        }
    }

    private void OnDisable()
    {
        if(health != null)
        {
            health.RemoveObserver(this);
        }
    }

    public void OnHealthChanged(int currentHealth, int maximumHealth)
    {
        healthSlider.maxValue = maximumHealth;
        healthSlider.value = currentHealth;
    }
}