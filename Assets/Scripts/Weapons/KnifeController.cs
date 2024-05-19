using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Scr_Models;

public class KnifeController : MonoBehaviour
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

    [Header("Shooting")]
    //Gun stats
    [SerializeField]
    private int damage;
    [SerializeField]
    private float timeBetweenShooting, spread, range, timeBetweenShots;
    [SerializeField]
    private int bulletsPerTap;

    //bools 
    bool shooting, readyToShoot;

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
    private Animator anim;

    [SerializeField]
    private TextMeshProUGUI text;

    [HideInInspector]
    public bool isAimingIn;

    [HideInInspector]
    public bool isShooting;


    void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialised)
        {
            return;
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        MyInput();

        text.SetText(" ");
    }

    private void Awake()
    {
        readyToShoot = true;
    }

    #region - Shooting -



    private void MyInput()
    {
        shooting = Input.GetKeyDown(KeyCode.Mouse0);

        //Shoot
        if (readyToShoot && shooting)
        {
            Shoot();
        }

        if (characterController.isSprinting)
        {
            shooting = false;
        }

        if (shooting)
        {
            anim.SetBool("attack", true);
        }
        else
        {
            anim.SetBool("attack", false);
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        // Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        Debug.Log($"Stab direction: {direction}"); // Debug the direction

        // Perform Raycast
        if (Physics.Raycast(fpsCam.transform.position, direction, out RaycastHit rayHit, range, whatIsEnemy))
        {
            Debug.Log($"Hit: {rayHit.collider.name}"); // Confirm hit

            GameObject bulletHolePrefab = bulletHoleGraphic; // Default bullet hole
            float bulletHoleLifetime = 0.75f;

            // Determine action based on the tag of the hit object
            if (rayHit.collider.CompareTag("Enemy"))
            {
                bool isDead = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);
                bulletHolePrefab = isDead ? bulletHoleLastShotEnemyGraphic : bulletHoleEnemyGraphic;
                bulletHoleLifetime = isDead ? Mathf.Infinity : 0.75f;
            }
            else if (rayHit.collider.CompareTag("Crate"))
            {
                bool isDestroyed = rayHit.collider.GetComponent<Damageable>().ApplyDamage(damage);
                if (isDestroyed) bulletHolePrefab = null; // No bullet hole if crate is destroyed
            }

            // Instantiate bullet hole if applicable
            if (bulletHolePrefab != null)
            {
                GameObject bulletHoleInstance = Instantiate(bulletHolePrefab, rayHit.point + rayHit.normal * 0.001f, Quaternion.LookRotation(rayHit.normal));
                if (bulletHoleLifetime != Mathf.Infinity)
                {
                    Destroy(bulletHoleInstance, bulletHoleLifetime);
                }
            }


            // Muzzle flash
            GameObject flashInstance = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
            Destroy(flashInstance, 1f); // Destroy the muzzle flash after 1 second
        }

        // Reset the readyToShoot flag after the specified time interval
        Invoke(nameof(ResetShot), timeBetweenShooting);
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }

    #endregion

    #region - Initialise -

    public void Initialise(CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInitialised = true;
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

        if (swayTime > 6.3f)
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
