using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private string weaponName;

    public string GetWeaponName()
    {
        return weaponName;
    }

    public abstract void Fire();
}
