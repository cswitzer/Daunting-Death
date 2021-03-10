using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Mover : MonoBehaviour
{
    // Character controller (adjustable params)
    [SerializeField] float walkingSpeed = 1.0f;
    [SerializeField] float runningSpeed = 2.0f;
    [SerializeField] float dodgingSpeed = 6.0f;
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
    private Vector3 playerVelocity;       // for handling gravity
    private bool isGrounded;          // checks if character controller is grounded
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isDodging = false;

    enum States
    {
        FreeMovement,
        LockedOn
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckIfGrounded();
        if (characterController.isGrounded)
            playerVelocity.y = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
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
            // update player animation slowly over time
            HandleMovementAnimations();

            // This code only helps our character face the right way...
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y /*points player in direction of camera*/;
            // gradually change to target angle in degrees over time
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //... but we also need to move in that direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // check if the character is dodging and adjust the speed to the dodging speed
            if (isDodging)
            {
                characterController.Move(moveDir.normalized * dodgingSpeed * Time.deltaTime);
                return;
            }

            // Check if the player is sprinting or just walking
            if (Input.GetKey(KeyCode.LeftShift))
                characterController.Move(moveDir.normalized * runningSpeed * Time.deltaTime);
            else
                characterController.Move(moveDir.normalized * walkingSpeed * Time.deltaTime);
        }
        else
        {
            if (isGrounded)
                RevertToIdleAnim();
        }
    }

    private void CheckIfGrounded()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            playerVelocity.y = gravity * Time.deltaTime;
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
    }
    private void HandleFalling()
    {
        if (!isGrounded)
        {
            // Physics formula for free falling
            playerVelocity.y += gravity * Mathf.Pow(Time.deltaTime, 2);
            characterController.Move(playerVelocity);
        }
    }

    private void HandleMovementAnimations()
    {
        // if the shift button is pressed down and the player is grounded, trigger sprint animation
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", false);

            // for dodging
            if (Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(PlayDodgingSequence());
            }
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", false);

            // for dodging
            if (Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(PlayDodgingSequence());
            }
        }
    }

    private void RevertToIdleAnim()
    {
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
    }

    private IEnumerator PlayDodgingSequence()
    {
        isDodging = true;

        // dodging animation will play here
        animator.SetBool("isDodging", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isIdle", false);

        // wait 2 seconds for the animation to finish playing
        yield return new WaitForSeconds(1);

        animator.SetBool("isDodging", false);
        isDodging = false;
    }

    // These function can be called everytime player's foot hits the ground
    private void FootL()
    {

    }

    private void FootR()
    {

    }
}
