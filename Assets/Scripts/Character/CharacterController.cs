using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Scr_Models;

public class CharacterController : MonoBehaviour
{
    private UnityEngine.CharacterController characterController;
    private DefaultInput defaultInput;
    [HideInInspector]
    public Vector2 input_Movement;
    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public new Transform camera;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;
    public LayerMask playerMask;
    public LayerMask groundMask;


    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;


    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    [HideInInspector]
    public bool isSprinting;

    [SerializeField] private float sprintCooldown = 0.5f; // Cooldown time in seconds before allowing sprint toggle again
    private float lastSprintToggleTime = 0f; // Tracks the last time sprint was toggled

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public WeaponController currentWeapon;
    public KnifeController currentWeaponKnife;
    public bool isKnifeSelected;
    public float weaponAnimationSpeed;

    [Header("Weapon Slots")]
    public GameObject Slot1;
    public GameObject Slot2;
    public GameObject Slot3;
    public GameObject Slot4;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;

    [Header("Leaning")]
    public Transform LeanPivot;
    private float currentLean;
    private float targetLean;
    public float leanAngle;
    public float leanSmoothing;
    private float leanVelocity;

    private bool isLeaningLeft;
    private bool isLeaningRight;
    private bool canLean = true;

    [Header("Aiming In")]
    public bool isAimingIn;


    public Image crosshairImage;

    public float interactionDistance = 3.0f;
    public GameObject starWithDiamonds; // The star model with colored diamonds
    public GameObject starWithoutDiamonds; // The initial star model
    public Canvas diamondsCanvas; // The canvas displaying collected diamonds
    public GameObject[] uiCanvases; // Array of UI canvases to disable

    public ParticleSystem portalParticles; // The particle system for the portal
    public Canvas levelCompletedCanvas; // The canvas showing level completion

    public Transform portalTransform; // The transform of the portal
    public float portalDetectionRadius = 2.0f; // The radius to detect player in por

    public Image darkenScreenImage; // Image for darkening the screen
    private bool isCompleted = false;


    #region - Awake -

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();

        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();

        defaultInput.Character.Sprint.performed += e => ToggleSprint();
        defaultInput.Character.SprintReleased.performed += e => StopSprint();

        defaultInput.Character.LeanLeftPressed.performed += e => isLeaningLeft = true;
        defaultInput.Character.LeanLeftReleased.performed += e => isLeaningLeft = false;

        defaultInput.Character.LeanRightPressed.performed += e => isLeaningRight = true;
        defaultInput.Character.LeanRightReleased.performed += e => isLeaningRight = false;

        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();

        defaultInput.Weapon.Fire1Pressed.performed += e => ShootingPressed();
        defaultInput.Weapon.Fire1Released.performed += e => ShootingReleased();

        defaultInput.Character.EquipWeapon1.performed += e => Equip1();
        defaultInput.Character.EquipWeapon2.performed += e => Equip2();
        defaultInput.Character.EquipWeapon3.performed += e => Equip3();
        defaultInput.Character.EquipWeapon4.performed += e => Equip4();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<UnityEngine.CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }

        if (currentWeaponKnife)
        {
            currentWeaponKnife.Initialise(this);
        }


        float adjustedValue = PlayerPrefs.GetFloat("crosshaircolor", 0);
        Color crosshairColor = Color.HSVToRGB(1f, 1f, 1 - adjustedValue);
        crosshairImage.color = crosshairColor; // Apply the new color
    }

    #endregion

    #region - Update -

    private void Update()
    {
        SetIsGrounded();
        SetIsFalling();

        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
        CalculateLeaning();
        CalculateAimingIn();
        CrosshairControl();
        CheckForDiamond();
        CheckForStarInteraction();
        CheckForPortalInteraction();
    }

    #endregion

    #region - Crosshair -

    private void CrosshairControl()
    {
        if (isAimingIn || isSprinting)
        {
            crosshairImage.enabled = false;
        }
        else if (input_Movement.magnitude > 0 || isFalling) // Assuming this is how you detect movement
        {
            crosshairImage.enabled = true;
            crosshairImage.rectTransform.sizeDelta = new Vector2(75, 75); // Enlarged size
        }
        else
        {
            crosshairImage.enabled = true;
            crosshairImage.rectTransform.sizeDelta = new Vector2(50, 50); // Normal size
        }
    }

    private void CrosshairColorChange(Color newCrosshairColour)
    {
        crosshairImage.color = newCrosshairColour;
    }

    #endregion

    #region - Shooting -

    private void ShootingPressed()
    {
        if (currentWeapon && !isSprinting)
        {
            currentWeapon.isShooting = true;
            
        }

        if (currentWeaponKnife && !isSprinting)
        {
            currentWeaponKnife.isShooting = true;

        }
    }

    private void ShootingReleased()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = false;
        }

        if (currentWeaponKnife)
        {
            currentWeaponKnife.isShooting = false;
        }
    }

    #endregion

    #region - GunSwitch -

    void Equip1()
    {
        canLean = true;
        Slot1.SetActive(true);
        Slot2.SetActive(false);
        Slot3.SetActive(false);
        Slot4.SetActive(false);

        // Update current weapon
        currentWeapon = Slot1.GetComponent<WeaponController>();
        if (currentWeapon != null)
            currentWeapon.Initialise(this);
    }

    void Equip2()
    {
        canLean = true;
        Slot1.SetActive(false);
        Slot2.SetActive(true);
        Slot3.SetActive(false);
        Slot4.SetActive(false);

        // Update current weapon
        currentWeapon = Slot2.GetComponent<WeaponController>();
        if (currentWeapon != null)
            currentWeapon.Initialise(this);
    }

    void Equip3()
    {
        canLean = true;
        Slot1.SetActive(false);
        Slot2.SetActive(false);
        Slot3.SetActive(true);
        Slot4.SetActive(false);

        // Update current weapon
        currentWeapon = Slot3.GetComponent<WeaponController>();
        if (currentWeapon != null)
            currentWeapon.Initialise(this);
    }

    
    void Equip4()
    {
        canLean = false;
        Slot1.SetActive(false);
        Slot2.SetActive(false);
        Slot3.SetActive(false);
        Slot4.SetActive(true);

        // Update current weapon
        currentWeaponKnife = Slot4.GetComponent<KnifeController>();
        if (currentWeaponKnife != null)
            currentWeaponKnife.Initialise(this);

    }

    

    #endregion

    #region - Awake -

    private void AimingInPressed()
    {
        if (!isSprinting)
        {
            isAimingIn = true;
        }
    }

    private void AimingInReleased()
    {
        isAimingIn = false;
    }

    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isAimingIn = isAimingIn;

    }


    #endregion

    #region - IsFalling / IsGrounded -

    private void SetIsGrounded()
    {
        isGrounded = Physics.CheckSphere(feetTransform.position, playerSettings.isGroundedRadius, groundMask);

        if (!isGrounded)
        {
            // Define the extra distance you're willing to consider the player as still being grounded.
            float extraGroundedDistance = 0.5f; // 0.5 meters above the ground

            RaycastHit hit;
            if (Physics.Raycast(feetTransform.position, Vector3.down, out hit, playerSettings.isGroundedRadius + extraGroundedDistance, groundMask))
            {
                isGrounded = true;
            }
        }
    }


    private void SetIsFalling()
    {
        isFalling = (!isGrounded && characterController.velocity.magnitude >= playerSettings.isFallingSpeed);
    }

    #endregion

    #region - View / Movement -

    private void CalculateView()
    {
        newCharacterRotation.y += (isAimingIn ? playerSettings.ViewXSensitivity * playerSettings.AimingSensitivityEffector : playerSettings.ViewXSensitivity) * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += (isAimingIn ? playerSettings.ViewYSensitivity * playerSettings.AimingSensitivityEffector : playerSettings.ViewYSensitivity) * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        if (characterController.velocity.magnitude > 3 && !isAimingIn)
        {
            isLeaningLeft = false;
            isLeaningRight = false;
        }

        var verticalSpeed = playerSettings.WalkingFowardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningFowardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
            isAimingIn = false;
        }

        if (!isGrounded)
        {
            playerSettings.SpeedEffector = playerSettings.FallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.SpeedEffector = playerSettings.ProneSpeedEffector;
        }
        else if (isAimingIn)
        {
            playerSettings.SpeedEffector = playerSettings.AimingSpeedEffector;
        }
        else
        {
            playerSettings.SpeedEffector = 1;
        }

        float effectiveVerticalSpeed = verticalSpeed * playerSettings.SpeedEffector;
        float effectiveHorizontalSpeed = horizontalSpeed * playerSettings.SpeedEffector;

        Vector3 inputVector = new Vector3(input_Movement.x, 0, input_Movement.y);
        inputVector = Vector3.ClampMagnitude(inputVector, 1); // Normalize input to prevent faster diagonal movement

        Vector3 targetMovement = new Vector3(effectiveHorizontalSpeed * inputVector.x, 0, effectiveVerticalSpeed * inputVector.z);
        targetMovement = transform.TransformDirection(targetMovement) * Time.deltaTime; // Apply smoothing

        weaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.WalkingFowardSpeed * playerSettings.SpeedEffector);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }

        if (!isGrounded)
        {
            if (playerGravity > gravityMin)
            {
                playerGravity -= gravityAmount * Time.deltaTime;
            }
        }
        else
        {
            playerGravity = Mathf.Max(playerGravity, -0.1f); // Reset gravity when grounded to avoid continuous acceleration.
        }

        targetMovement.y += playerGravity; // Apply gravity effect to vertical movement.

        targetMovement += jumpingForce * Time.deltaTime;

        characterController.Move(targetMovement);
    }



    #endregion

    #region - Leaning -



    private void CalculateLeaning()
    {

            if (isLeaningLeft && canLean)
            {
                targetLean = leanAngle;
            }
            else if (isLeaningRight && canLean)
            {
                targetLean = -leanAngle;
            }
            else
            {
                targetLean = 0;
            }

            if (playerStance == PlayerStance.Crouch || playerStance == PlayerStance.Prone)
            {
                targetLean = 0;
            }

            currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);

            LeanPivot.localRotation =  Quaternion.Euler(new Vector3(0, 0, currentLean));
    }

    #endregion

    #region - Jumping -

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        // Check if grounded
        if (!isGrounded) return;

        // Handle the stance before attempting to jump
        if (playerStance == PlayerStance.Prone || playerStance == PlayerStance.Crouch)
        {
            if (CanStand())
            {
                // If player can stand and they are trying to jump, switch to stand but don't jump yet
                playerStance = PlayerStance.Stand;
            }
            return; // Exit the method without jumping
        }

        // If the player is standing and there are no obstructions above, they can jump
        if (playerStance == PlayerStance.Stand)
        {
            // Apply jumping physics
            jumpingForce = Vector3.up * playerSettings.JumpingHeight;
            playerGravity = 0;
            currentWeapon.TriggerJump();
        }
    }




    #endregion

    #region - Stance -

    private void CalculateStance()
    {
        if (isSprinting) return;

        var currentStance = playerStandStance;

        if(playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;

        }
        else if (playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }

        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (!CanStand())
            {
                return; // Don't stand if there's no space
            }
            playerStance = PlayerStance.Stand; // Stand up if there is space
        }
        else if (playerStance == PlayerStance.Prone)
        {
            if (!CanCrouch())
            {
                return; // Don't crouch if there's no space
            }
            playerStance = PlayerStance.Crouch; // Crouch down if there is space
        }
        else
        {
            playerStance = PlayerStance.Crouch; // Crouch down
        }
    }



    private void Prone()
    {
        playerStance = PlayerStance.Prone;
    }

    private bool CanStand()
    {
        // Position to start the raycast (just above the player's current head position)
        Vector3 rayStart = cameraHolder.position + Vector3.up * 0.1f;
        // Length of the ray (how tall the player stands)
        float rayLength = playerStandStance.StanceCollider.height - 0.1f; // slight offset from current position

        // Cast a ray upwards to check for space to stand up
        bool isBlocked = Physics.Raycast(rayStart, Vector3.up, rayLength, playerMask);
        return !isBlocked; // Return true if not blocked
    }

    private bool CanCrouch()
    {
        // Position to start the raycast (just above the player's current head position)
        Vector3 rayStart = cameraHolder.position + Vector3.up * 0.1f;
        // Length of the ray (how tall the player is when crouching)
        float rayLength = playerCrouchStance.StanceCollider.height - 0.1f; // slight offset from current position

        // Cast a ray upwards to check for space to crouch
        bool isBlocked = Physics.Raycast(rayStart, Vector3.up, rayLength, playerMask);
        return !isBlocked; // Return true if not blocked
    }


    #endregion

    #region - Sprinting -

    private void ToggleSprint()
    {
        // Check for cooldown to prevent rapid toggling
        if (Time.time - lastSprintToggleTime < sprintCooldown)
            return;

        if (input_Movement.y <= 0.2f || playerStance == PlayerStance.Crouch || playerStance == PlayerStance.Prone)
        {
            isSprinting = false;
            return;
        }

        isSprinting = !isSprinting;
        lastSprintToggleTime = Time.time; // Update the last toggle time
    }

    private void StopSprint()
    {
        if (playerSettings.sprintingHold)
        {
            isSprinting = false;
        }
    }


    #endregion

    #region - Diamond -

    private void CheckForDiamond()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Vector3 rayOrigin = Camera.main.transform.position;
            Vector3 rayDirection = Camera.main.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, 3.0f))
            {
                if (hitInfo.collider.CompareTag("Diamond"))
                {
                    Diamond diamond = hitInfo.collider.GetComponent<Diamond>();
                    if (diamond != null)
                    {
                        DiamondCollection.Instance.CollectDiamond(diamond.diamondType);
                        Destroy(hitInfo.collider.gameObject); // Remove the diamond from the scene
                    }
                }
            }
        }
    }

    #endregion

    #region - LevelEnd -

    private void CheckForStarInteraction()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Vector3 rayOrigin = Camera.main.transform.position;
            Vector3 rayDirection = Camera.main.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, interactionDistance))
            {
                if (hitInfo.collider.CompareTag("Star") && !isCompleted)
                {
                    if (DiamondCollection.Instance.AllDiamondsCollected())
                    {
                        CompleteLevel();
                    }
                    else
                    {
                        ShowFindAllDiamondsMessage();
                    }
                }
            }
        }
    }

    private void CompleteLevel()
    {
        isCompleted = true;
        starWithoutDiamonds.SetActive(false);
        starWithDiamonds.SetActive(true);
        diamondsCanvas.gameObject.SetActive(false); // Disable the DiamondsCanvas
        ActivatePortal();
    }

    private void ActivatePortal()
    {
        if (portalParticles != null)
        {
            portalParticles.Play(); // Activate the particle system of the portal
            Debug.Log("Portal particles activated.");
        }
        else
        {
            Debug.LogError("Portal particles not assigned in the inspector.");
        }
    }

    private void ShowFindAllDiamondsMessage()
    {
        // Optional: Show a message that all diamonds need to be collected
        Debug.Log("You need to collect all diamonds first!");
    }

    private void CheckForPortalInteraction()
    {
        if (isCompleted && portalParticles.isPlaying)
        {
            float distanceToPortal = Vector3.Distance(transform.position, portalTransform.position);
            if (distanceToPortal <= portalDetectionRadius)
            {
                if (AllEnemiesKilled())
                {
                    StartCoroutine(AnimatePortalEntry());
                }
                else
                {
                    // Optional: Show a message indicating that all enemies need to be killed
                    Debug.Log("You need to kill all enemies first!");
                }
            }
        }
    }

    private bool AllEnemiesKilled()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length == 0;
    }

    private IEnumerator AnimatePortalEntry()
    {
        // Disable all UI canvases
        foreach (var canvas in uiCanvases)
        {
            canvas.SetActive(false);
        }

        // Spin and shrink animation
        float spinShrinkDuration = 1f; // Duration of spin and shrink
        float elapsedTime = 0.0f;

        while (elapsedTime < spinShrinkDuration)
        {
            float t = elapsedTime / spinShrinkDuration;
            Camera.main.transform.Rotate(Vector3.forward, 3 * Time.deltaTime); // Slower rotation
            Camera.main.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t); // Slower shrink

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Start fading to black
        yield return StartCoroutine(FadeOut());

        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show level completed canvas after fade-out is complete
        ShowLevelCompleted();
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0.0f;
        Color originalColor = darkenScreenImage.color;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            darkenScreenImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0, 1, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }




    private void ShowLevelCompleted()
    {
        levelCompletedCanvas.gameObject.SetActive(true);
        // Add any additional logic for level completion, such as loading the next level
    }

    #endregion

    #region - Gizmos -

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);
    }

    #endregion
}
