using UnityEngine;
using UnityEngine.InputSystem;

public class HealthBasic : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 100;
    [SerializeField] private HealthBarBasic healthBar;
    [SerializeField] private DamageFeedBackBasic damageFeedback;
    [SerializeField] private HealthLoggerBasic healthLogger;

    private int currentHealth;

    [SerializeField] private NIS controls;

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
        healthBar.UpdateHealth(currentHealth, maximumHealth);
    }

    private void OnDamage(InputAction.CallbackContext context)
    {
        TakeDamage(10);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if(currentHealth < 0)
        {
            currentHealth = 0;
        }

        healthBar.UpdateHealth(currentHealth, maximumHealth);
        damageFeedback.ShowDamage(damage);
        healthLogger.LogHealth(currentHealth, maximumHealth);

        Debug.Log("Vida actual: " + currentHealth);
    }


    
}
