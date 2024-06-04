using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource component that will play the sound
    public AudioClip walkingClip; // The AudioClip for the walking sound
    public float walkStepInterval = 0.5f; // Interval between footstep sounds while walking
    public float runStepInterval = 10f; // Interval between footstep sounds while running
    public float footstepVolume = 1.0f; // Volume of the footstep sounds
    private float stepTimer; // Timer to manage the intervals

    private CharacterController characterController; // Reference to the CharacterController
    private bool isWalking; // Flag to check if the player is walking
    private bool isRunning; // Flag to check if the player is running

    private Vector3 previousPosition; // To track the previous position of the player
    private float currentSpeed; // To calculate the player's speed

    void Start()
    {
        characterController = GetComponentInParent<CharacterController>(); // Get the CharacterController from the parent
        audioSource.clip = walkingClip; // Assign the walking sound clip to the AudioSource
        audioSource.volume = footstepVolume; // Set the initial volume
        previousPosition = characterController.transform.position; // Set the initial position
    }

    void Update()
    {
        CalculateSpeed(); // Calculate the player's speed
        HandleFootsteps(); // Handle the footstep sounds
    }

    private void CalculateSpeed()
    {
        // Calculate the speed based on the change in position over time
        Vector3 currentPosition = characterController.transform.position;
        currentSpeed = (currentPosition - previousPosition).magnitude / Time.deltaTime;
        previousPosition = currentPosition;
    }

    private void HandleFootsteps()
    {
        // Determine if the player is walking or running based on the speed
        isWalking = currentSpeed > 0.1f && currentSpeed < 5f; // Adjust these values based on your walking speed
        isRunning = currentSpeed >= 5f; // Adjust this value based on your running speed

        // Check if the player is grounded
        bool isGrounded = characterController.isGrounded;

        if ((isWalking || isRunning) && isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                float interval = isRunning ? runStepInterval : walkStepInterval;
                PlayFootstepSound(isRunning ? 1.3f : 1.0f, interval, footstepVolume);
            }
        }
        else
        {
            stepTimer = 0f; // Reset timer if not walking, running, or grounded
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // Stop the sound if the player is not moving or not grounded
            }
        }
    }

    private void PlayFootstepSound(float pitch, float interval, float volume)
    {
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.Play();
        stepTimer = interval; // Reset the step timer based on walking or running interval
    }
}
