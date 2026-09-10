using UnityEngine;
using UnityEngine.UI;

public class DamageFeedbackObserver : MonoBehaviour, IHealthObserver
{
    [SerializeField] private Health health;
    [SerializeField] private Text damageText;

    private int previousHealth;

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
        int damge = previousHealth - currentHealth;
        if (damge > 0)
        {
            damageText.text = "-" + damage + " HP";
        }

        previousHealth = currentHealth;
    }
}
