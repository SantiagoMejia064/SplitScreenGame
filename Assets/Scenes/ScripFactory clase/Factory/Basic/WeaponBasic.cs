using UnityEngine;

public class WeaponBasic : MonoBehaviour
{
    [SerializeField] private WeaponType weaponType;

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}