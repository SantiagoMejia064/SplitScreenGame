using UnityEngine;

public class WeaponFactory : MonoBehaviour
{
    [SerializeField] private Weapon pistolPrefab;
    [SerializeField] private Weapon shotgunPrefab;
    [SerializeField] private Weapon rocketLauncherPrefab;
    
    public Weapon CreateWeapon(WeaponTypeEnum weaponType, Transform spawnPoint)
    {
        Weapon weaponPrefab = null;

        switch (weaponType)
        {
            case WeaponTypeEnum.Pistol:
                weaponPrefab = pistolPrefab;
                break;

            case WeaponTypeEnum.Shotgun:
                weaponPrefab = shotgunPrefab;
                break;

            case WeaponTypeEnum.RocketLauncher:
                weaponPrefab = rocketLauncherPrefab;
                break;
        }

        if (weaponPrefab != null)
        {
            Debug.LogError("No existe un prefab configurado para: " + weaponType);
            return null;
        }

        return Instantiate(weaponPrefab, spawnPoint.position, spawnPoint.rotation); 
    }
}