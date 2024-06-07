using static Scr_Models;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;


public enum WeaponType { Rifle, Shotgun, Pistol }

public class WeaponController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("References")]
    [SerializeField]
    private Animator weaponAnimator;
    [SerializeField]
    private Transform bulletSpawn;

    [Header("Settings")]
    [SerializeField]
    private WeaponSettingsModel settings;

    bool isInitialised;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;

    [SerializeField]
    private float fallingDelay;

    [Header("Weapon Breathing")]

    [SerializeField]
    private Transform weaponSwayObject;

    [SerializeField]
    private float swayAmountA = 1;
    [SerializeField]
    private float swayAmountB = 2;
    [SerializeField]
    private float swayScale = 250;
    [SerializeField]
    private float swayLerpSpeed = 14;

    private float swayTime;
    private Vector3 swayPosition;

    [Header("Sights")]
    [SerializeField]
    private Transform sightTarget;
    [SerializeField]
    private float sightOffset;
    [SerializeField]
    private float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;

    [Header("Shooting")]
    //Gun stats
    [SerializeField]
    private int damage;
    [SerializeField]
    private float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    [SerializeField]
    private int magazineSize, bulletsPerTap;
    [SerializeField]
    private bool allowButtonHold;
    int bulletsLeft, bulletsShot;
    private int bulletsTotal = 30; // Total ammo available

    //bools 
    bool shooting, readyToShoot, reloading;

    //Reference
    [SerializeField]
    private Camera fpsCam;
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private RaycastHit rayHit;
    [SerializeField]
    private LayerMask whatIsEnemy;

    //Graphics
    [SerializeField]
    private GameObject muzzleFlash, bulletHoleGraphic, bulletHoleEnemyGraphic, bulletHoleLastShotEnemyGraphic, bulletHoleEnemyGraphicBlowUp, bulletHoleLastShotEnemyGraphicBlowUp;
    [SerializeField]
    private TextMeshProUGUI text;
    private Animator anim;


    [HideInInspector]
    public bool isAimingIn;

    [HideInInspector]
    public bool isShooting;

    [HideInInspector]
    public bool isSprinting;


    public static List<WeaponController> AllWeapons = new List<WeaponController>();
    public WeaponType weaponType;

    [SerializeField]
    private AudioClip shootSound; // Shooting sound clip
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip ammoPickupSound; // Ammo pickup sound clip


    #region - Start / Update / Awake -

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;

        anim = GetComponent<Animator>();
    }


    private void Awake()
    {
        bulletsLeft = magazineSize;
        bulletsTotal = Mathf.Max(bulletsTotal, magazineSize); // Initialize total bullets, ensuring it's at least one full magazine
        readyToShoot = true;
        RegisterWeapon();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void RegisterWeapon()
    {
        if (!AllWeapons.Contains(this))
        {
            AllWeapons.Add(this);
        }
    }

    private void UnregisterWeapon()
    {
        AllWeapons.Remove(this);
    }

    private void OnDestroy()
    {
        UnregisterWeapon();
    }

    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();

        MyInput();

        // Update UI Text to show current bullets and total reserve
        text.SetText($"{bulletsLeft} / {magazineSize} Reserve: {bulletsTotal}");
    }


    #endregion

    #region - Shooting -



    private void MyInput()
    {
        CheckForAmmoBox();  // Continue checking for ammo box pickups

        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading && bulletsTotal > 0)
            Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && characterController.isSprinting == false)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }

        // Ensure reload animation is properly managed
        if (reloading && !isAimingIn && !isShooting && isGroundedTrigger == true)
        {
            anim.SetBool("reload", true);
        }
        else
        {
            anim.SetBool("reload", false);
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Play shooting sound
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        bool hitValidTarget = false;
        RaycastHit rayHit;

        // Continue raycasting until a valid target is hit or the maximum range is reached
        Vector3 rayOrigin = fpsCam.transform.position;
        float remainingRange = range;

        while (remainingRange > 0 && !hitValidTarget)
        {
            if (Physics.Raycast(rayOrigin, direction, out rayHit, remainingRange))
            {
                // Check if the hit collider has the "NoShoot" tag
                if (rayHit.collider.CompareTag("NoShoot"))
                {
                    // Adjust the ray origin to continue raycasting from the hit point
                    rayOrigin = rayHit.point + direction.normalized * 0.1f; // Move slightly forward to avoid hitting the same point again
                    remainingRange -= Vector3.Distance(fpsCam.transform.position, rayHit.point); // Reduce the remaining range
                }
                else
                {
                    hitValidTarget = true;

                    Debug.Log(rayHit.collider.name);

                    bool isEnemyKilled = false;
                    GameObject bulletHolePrefab = bulletHoleGraphic; // Default to the environment bullet hole
                    bool destroyBulletHole = false;

                    if (rayHit.collider.CompareTag("Enemy"))
                    {
                        // Apply damage and check if it resulted in enemy's death
                        isEnemyKilled = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);

                        // Use the enemy bullet hole graphic if hit an enemy
                        bulletHolePrefab = isEnemyKilled ? bulletHoleLastShotEnemyGraphic : bulletHoleEnemyGraphic;
                    }

                    if (rayHit.collider.CompareTag("EnemyBlowUp"))
                    {
                        // Apply damage and check if it resulted in enemy's death
                        isEnemyKilled = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);

                        // Use the enemy bullet hole graphic if hit an enemy
                        bulletHolePrefab = isEnemyKilled ? bulletHoleLastShotEnemyGraphicBlowUp : bulletHoleEnemyGraphicBlowUp;
                    }

                    // Define the bulletHolePrefab before using it in the conditions
                    GameObject bulletHole = null;

                    if (rayHit.collider.CompareTag("Crate"))
                    {
                        destroyBulletHole = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);
                        if (destroyBulletHole)
                        {
                            bulletHolePrefab = null; // Do not instantiate bullet hole if crate is destroyed
                        }
                    }

                    // Instantiate the bullet hole prefab only if it is not null
                    if (bulletHolePrefab != null)
                    {
                        bulletHole = Instantiate(bulletHolePrefab, rayHit.point + rayHit.normal * 0.001f, Quaternion.LookRotation(rayHit.normal));

                        float bulletHoleLifetime;
                        if (bulletHolePrefab == bulletHoleLastShotEnemyGraphicBlowUp)
                        {
                            bulletHoleLifetime = 0.5f;  // Short lifetime for the special last shot bullet hole
                        }
                        else
                        {
                            bulletHoleLifetime = isEnemyKilled ? 5.0f : 0.75f;  // Adjust these times as needed for other cases
                        }

                        // Schedule the bullet hole's destruction
                        Destroy(bulletHole, bulletHoleLifetime);
                    }
                }
            }
            else
            {
                // Exit the loop if no collider is hit within the remaining range
                break;
            }
        }

        // Muzzle flash graphics
        GameObject flashInstance = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        Destroy(flashInstance, 1f); // Destroy the muzzle flash after 1 second

        bulletsLeft--;
        bulletsShot--;

        Invoke(nameof(ResetShot), timeBetweenShooting); // Reset the shooting mechanism after the specified time

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots); // Continue shooting if there are bullets left and more shots to fire
    }




    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        int bulletsToLoad = Mathf.Min(bulletsTotal, magazineSize - bulletsLeft);
        if (bulletsToLoad > 0 && !isAimingIn && !isShooting)
        {
            reloading = true;
            Invoke(nameof(ReloadFinished), reloadTime);
        }
    }

    private void ReloadFinished()
    {
        int bulletsToLoad = Mathf.Min(bulletsTotal, magazineSize - bulletsLeft);
        bulletsLeft += bulletsToLoad;
        bulletsTotal -= bulletsToLoad;
        reloading = false;
    }

    public void AddAmmo(int amount)
    {
        bulletsTotal += amount;
    }

    private void CheckForAmmoBox()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Vector3 rayOrigin = Camera.main.transform.position;
            Vector3 rayDirection = Camera.main.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, 3.0f))
            {
                WeaponController weaponToRefill = null;
                int ammoToAdd = 0;

                // Determine the type of ammo and the amount to add
                switch (hitInfo.collider.tag)
                {
                    case "RifleAmmo":
                        weaponToRefill = FindWeaponController(WeaponType.Rifle);
                        ammoToAdd = 30;
                        break;
                    case "ShotgunAmmo":
                        weaponToRefill = FindWeaponController(WeaponType.Shotgun);
                        ammoToAdd = 14;
                        break;
                    case "PistolAmmo":
                        weaponToRefill = FindWeaponController(WeaponType.Pistol);
                        ammoToAdd = 7;
                        break;
                }

                // If we found a weapon and have a valid ammo amount, add the ammo
                if (weaponToRefill != null && ammoToAdd > 0)
                {
                    weaponToRefill.AddAmmo(ammoToAdd);
                    Destroy(hitInfo.collider.gameObject);  // Destroy the ammo box

                    // Play ammo pickup sound
                    if (ammoPickupSound != null)
                    {
                        audioSource.PlayOneShot(ammoPickupSound);
                    }
                }
            }
        }
    }


    // Utility method to find a weapon controller by type
    private WeaponController FindWeaponController(WeaponType type)
    {
        foreach (WeaponController weapon in FindObjectsOfType<WeaponController>())
        {
            if (weapon.weaponType == type)
                return weapon;
        }
        return null;  // No weapon of the given type found
    }

    #endregion

    #region - Initialise -

    public void Initialise(CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInitialised = true;
    }

    #endregion

    #region - Aiming In -

    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;

        if (isAimingIn)
        {
            targetPosition = characterController.camera.transform.position + (weaponSwayObject.transform.position - sightTarget.transform.position) + (characterController.camera.transform.forward * sightOffset);         
        }
        

        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition + swayPosition;
    }

    #endregion

    #region - Jumping -

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }

    #endregion

    #region - Rotation -

    private void CalculateWeaponRotation()
    {
        targetWeaponRotation.y += (isAimingIn ? settings.SwayAmount / 3 : settings.SwayAmount) * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += (isAimingIn ? settings.SwayAmount / 3 : settings.SwayAmount) * (settings.SwayYInverted ? characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampY);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampX, settings.SwayClampY);
        targetWeaponRotation.z = isAimingIn ? 0 : targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        targetWeaponMovementRotation.z = (isAimingIn ? settings.MovementSwayX / 3 : settings.MovementSwayX) * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = (isAimingIn ? settings.MovementSwayY / 3 : settings.MovementSwayY) * (settings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    #endregion

    #region - Animations -

    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }

        if (!characterController.isGrounded && isGroundedTrigger)
        {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }


        weaponAnimator.SetBool("isSprinting", characterController.isSprinting);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);
    }

    #endregion

    #region - Sway -

    private void CalculateWeaponSway()
    {

        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (isAimingIn ? swayScale * 3 : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if(swayTime > 6.3f)
        {
            swayTime = 0;
        }

    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    #endregion
}
