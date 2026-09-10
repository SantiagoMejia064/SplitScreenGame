using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Health : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 100;

    private int currentHealth;

    private List<IHealthObserver> observers = new List<IHealthObserver>();

    private NIS controls;

    private void Awake()
    {
        controls = new NIS();
        currentHealth = maximumHealth;
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Damage.performed += OnDamage;
    }

    private void OnDisable()
    {
        controls.Player.Damage.performed -= OnDamage;

        controls.Player.Disable();
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void OnDamage(InputAction.CallbackContext context)
    {
        TakeDamage(10);
    }

    private void AddObserver(IHealthObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IHealthObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        foreach (IHealthObserver observer in observers)
        {
            observer.OnHealthChanged(currentHealth, maximumHealth);
        }

        Debug.Log("Vida actual: " + currentHealth);
    }

}
