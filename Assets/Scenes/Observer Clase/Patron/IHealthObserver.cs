public interface IHealthObserver
{
    void OnHealthChanged(int currentHealth, int maximumHealth);
}