using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSpawnerBasic : MonoBehaviour
{
    [SerializeField] private GameObject pistolPrefab;
    [SerializeField] private GameObject shotgunPrefab;
    [SerializeField] private GameObject rocketLauncherPrefab;

    [SerializeField] private Transform spawnPoint;

    private NIS controls;

    private GameObject currentWeapon;

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
    }

    private void OnDisable()
    {
        controls.Player.Pistol.performed -= OnPistol;
        controls.Player.Shotgun.performed -= OnShotgun;
        controls.Player.RocketLauncher.performed -= OnRocketLauncher;

        controls.Disable();
    }

    private void OnPistol(InputAction.CallbackContext context)
    {
        CreatePistol();
    }

    private void OnShotgun(InputAction.CallbackContext context)
    {
        CreateShotgun();
    }

    private void OnRocketLauncher(InputAction.CallbackContext context)
    {
        CreateRocketLauncher();
    }

    private void CreatePistol()
    {
        DestroyCurrentWeapon();
        currentWeapon = Instantiate(pistolPrefab, 
        spawnPoint.position, spawnPoint.rotation);

        currentWeapon.transform.SetParent(spawnPoint.transform);
    }

    private void CreateShotgun()
    {
        DestroyCurrentWeapon();
        currentWeapon = Instantiate(shotgunPrefab, 
        spawnPoint.position, spawnPoint.rotation);

        currentWeapon.transform.SetParent(spawnPoint.transform);
    }

    private void CreateRocketLauncher()
    {
        DestroyCurrentWeapon();
        currentWeapon = Instantiate(rocketLauncherPrefab, 
        spawnPoint.position, spawnPoint.rotation);

        currentWeapon.transform.SetParent(spawnPoint.transform);
    }

    private void DestroyCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
    }



}
