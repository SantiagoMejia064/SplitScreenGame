using UnityEngine;

public class WeaponBasic : MonoBehaviour
{
    [SerializeField] private WeaponTypeEnum weaponType;

    public WeaponTypeEnum GetWeaponType()
    {
        return weaponType;
    }
}
