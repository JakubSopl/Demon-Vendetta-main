using System;
using System.Collections.Generic;
using UnityEngine;


public static class Scr_Models
{
    #region - Player -

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }

    public enum WeaponType
    {
        Rifle,
        Shotgun,
        Pistol
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public float AimingSensitivityEffector;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement Settings")]
        public bool sprintingHold;
        public float MovementSmoothing;

        [Header("Movement - Running")]
        public float RunningFowardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingFowardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;
        public float FallingSmoothing;

        [Header("Speed Effectors")]
        public float SpeedEffector = 1;
        public float CrouchSpeedEffector;
        public float ProneSpeedEffector;
        public float FallingSpeedEffector;
        public float AimingSpeedEffector;

        [Header("Is Grounded / Falling")]
        public float isGroundedRadius;
        public float isFallingSpeed;

        public float FallGravityMultiplier { get; internal set; }
        public float MaxFallGravityMultiplier { get; internal set; }
    }

    [Serializable]
    public class CharacterStance
    {
        public float CameraHeight;      
        public CapsuleCollider StanceCollider;
    }

    #endregion

    #region - Weapons -

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Weapon Sway")]
        public float SwayAmount;
        public bool SwayYInverted;
        public bool SwayXInverted;
        public float SwaySmoothing;
        public float SwayResetSmoothing;
        public float SwayClampX;
        public float SwayClampY;

        [Header("Weapon Movement Sway")]
        public float MovementSwayX;
        public float MovementSwayY;
        public bool MovementSwayYInverted;
        public bool MovementSwayXInverted;
        public float MovementSwaySmoothing;
    }


    #endregion
}
