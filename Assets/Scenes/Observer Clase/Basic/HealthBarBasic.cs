using UnityEngine;
using UnityEngine.UI;

public class HealthBarBasic : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    public void UpdateHealth(int currentHealth, int maximumHealth)
    {
        healthSlider.maxValue = maximumHealth;
        healthSlider.value = currentHealth;
    }
}
