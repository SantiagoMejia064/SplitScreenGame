using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSpawnerFactory : MonoBehaviour
{
   [SerializeField] private WeaponFactory WeaponFactory;

   [SerializeField] private Transform spawnPoint;

   private NIS controls;

   private Weapon currentWeapon;

   private void Awake()
   {
       controls = new NIS();
       
   }

   private void OnEnable()
   {
       controls.Enable();

       controls.Player.Pistol.performed += OnPistol;
       controls.Player.Shotgun.performed += OnShotgun;
       controls.Player.RocketLauncher.performed += OnRocketLauncher;

       controls.Player.Fire.performed += OnFire;
   }

   private void OnDisable()
   {
       controls.Player.Pistol.performed -= OnPistol;
       controls.Player.Shotgun.performed -= OnShotgun;
       controls.Player.RocketLauncher.performed -= OnRocketLauncher;

       controls.Player.Fire.performed -= OnFire;

       controls.Disable();
   }

   private void OnPistol(InputAction.CallbackContext context)
   {
       CreateWeapon(WeaponType.Pistol);
   }

    private void OnShotgun(InputAction.CallbackContext context)
    {
         CreateWeapon(WeaponType.Shotgun);
    }

    private void OnRocketLauncher(InputAction.CallbackContext context)
    {
         CreateWeapon(WeaponType.RocketLauncher);
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        FireCurrentWeapon();
    }

    private void CreateWeapon(WeaponType weaponType)
    {
        DestroyCurrentWeapon();

        currentWeapon = WeaponFactory.CreateWeapon(weaponType, spawnPoint);

        currentWeapon.transform.SetParent(spawnPoint.transform);

        if (currentWeapon != null)
        {
            Debug.Log("Arma creada: " + currentWeapon.GetType().Name);
        }
    }

    private void DestroyCurrentWeapon()
    {
       if (currentWeapon != null)
       {
           Destroy(currentWeapon.gameObject);
           currentWeapon = null;
       }
    }

    private void FireCurrentWeapon()
    {
        if(currentWeapon != null)
        {
            currentWeapon.Fire();
        }
    }


}
