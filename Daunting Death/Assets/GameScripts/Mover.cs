using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Mover : MonoBehaviour
{
    // Character controller (adjustable params)
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float acceleration = 6.0f;
    [SerializeField] Transform camera;
    
    // Components for player
    private CharacterController characterController;
    private Animator animator;

    // Character Movement Tweaks
    private float turnSmoothVelocity;
    private float currentSpeed = 0.0f;
    private float deceleration = 10.0f;
    private Vector3 playerVelocity;       // for handling gravity
    private bool groundedPlayer;          // checks if character controller is grounded

    // Animation
    public float animVelocity = 0.0f;     // for blend tree
    public float animAcceleration = .1f; // for blend tree
    public float animDeceleration = 0.5f; // for blend tree
    private int velocityHash;             // for blend tree

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        velocityHash = Animator.StringToHash("Velocity");
    }

    void Update()
    {
        MovePlayer();
        CheckIfGrounded();
        HandleFalling();
    }

    private void MovePlayer()
    {
        // a direction is only processed if input keys for movement are pressed
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // magnitude of direction checks if the character is actually moving
        if (direction.magnitude >= 0.1)
        {
            // slowly accelerate player
            BuildUpMovementSpeed();

            // update player animation slowly over time
            UpdateIdleToRunAnimator();

            // This code only helps our character face the right way...
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y /*points player in direction of camera*/;
            // gradually change to target angle in degrees over time
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //... but we also need to move in that direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            HandleMovementDeceleration();

            UpdateRunToIdleAnimator();
        }
    }

    private void CheckIfGrounded()
    {
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

    }
    private void HandleFalling()
    {
        if (!groundedPlayer)
        {
            // Physics formula for free falling
            playerVelocity.y += gravity * Mathf.Pow(Time.deltaTime, 2);
            characterController.Move(playerVelocity);
        }
    }

    private void UpdateIdleToRunAnimator()
    {
        if (animVelocity < 1)
        {
            animVelocity += animAcceleration * (Time.deltaTime * 4);
            animator.SetFloat(velocityHash, animVelocity);
        }
    }

    private void UpdateRunToIdleAnimator()
    {
        if (animVelocity > Mathf.Epsilon)
        {
            animVelocity -= animDeceleration * (Time.deltaTime * 3);
            animator.SetFloat(velocityHash, animVelocity);
        }
    }

    private void BuildUpMovementSpeed()
    {
        // slowly increment the speed of the player as time goes on
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
    }

    private void HandleMovementDeceleration()
    {
        // if current speed is over zero and movement keys are not pressed
        if (currentSpeed > 0)
        {
            currentSpeed -= deceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0;
        }
    }

    private void FootL()
    {

    }

    private void FootR()
    {

    }

}
