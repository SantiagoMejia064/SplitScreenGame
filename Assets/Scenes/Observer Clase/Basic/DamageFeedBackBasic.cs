using UnityEngine;
using UnityEngine.UI;

public class DamageFeedBackBasic : MonoBehaviour
{
    [SerializeField] private Text damageText;

    public void ShowDamage(int damage)
    {
        damageText.text = "-" + damage + " HP";
        Debug.Log("Feedback de daño: -" + damage + " HP");
    }
}
