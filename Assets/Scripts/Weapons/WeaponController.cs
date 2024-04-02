using static Scr_Models;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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
    private GameObject muzzleFlash, bulletHoleGraphic, bulletHoleEnemyGraphic, bulletHoleLastShotEnemyGraphic;
    [SerializeField]
    private TextMeshProUGUI text;
    private Animator anim;


    [HideInInspector]
    public bool isAimingIn;

    [HideInInspector]
    public bool isShooting;

    [HideInInspector]
    public bool isSprinting;





    #region - Start / Update / Awake -

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;

        anim = GetComponent<Animator>();
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
        //CalculateShooting();

        MyInput();

        //SetText
        text.SetText(bulletsLeft + " / " + magazineSize);
    }

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    #endregion

    #region - Shooting -

    

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
      
        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && characterController.isSprinting == false)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }

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

        // Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        // RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out RaycastHit rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            bool isEnemyKilled = false;
            GameObject bulletHolePrefab = bulletHoleGraphic; // Default to the environment bullet hole

            if (rayHit.collider.CompareTag("Enemy"))
            {
                // Apply damage and check if it resulted in enemy's death
                isEnemyKilled = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);

                // Use the enemy bullet hole graphic if hit an enemy
                // Check if the enemy is killed to decide which bullet hole prefab to use
                bulletHolePrefab = isEnemyKilled ? bulletHoleLastShotEnemyGraphic : bulletHoleEnemyGraphic;
            }

            // Instantiate the bullet hole prefab whether the enemy was killed or not
            GameObject bulletHole = Instantiate(bulletHolePrefab, rayHit.point + rayHit.normal * 0.001f, Quaternion.LookRotation(rayHit.normal));
            // If it's an enemy, we also check to potentially destroy the bullet hole quickly, except for the last shot graphic
            if (rayHit.collider.CompareTag("Enemy") && !isEnemyKilled)
            {
                Destroy(bulletHole, 0.75f); // Destroy quickly if it's an enemy and not the last shot
            }
        }

        // Muzzle flash graphics
        GameObject flashInstance = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        Destroy(flashInstance, 1f);

        bulletsLeft--;
        bulletsShot--;



        Invoke(nameof(ResetShot), timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        if (isAimingIn || isShooting)
        {
            reloading = false;
        }
        else
        {
            reloading = true;
            Invoke(nameof(ReloadFinished), reloadTime);
        }
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
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
