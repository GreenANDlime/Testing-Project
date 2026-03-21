using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
   [Header("Movement")]
    public float moveSpeed;
    public float defaultSpeed = 3;
    public float runSpeed = 5;
    public float drag = 4f;

    [Header("Running")]
    public float maxRunTime = 5f;
    private float currentRunTime;
    private bool isRunning;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale = 0.5f;
    public float startYScale;

    [Header("Jumping & Gravity")]
    public float jumpForce = 16f;
    public float gravityMultiplier = 2f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    private bool isGrounded;
  

    [Header("Orientation")]
    public Transform orientation; // Reference for movement direction

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;



    private void Awake()
    {
        startYScale = transform.localScale.y;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentRunTime = maxRunTime;
    }
   
    private void Update()
    {
        HandleInput();
        HandleRunning();
        HandleJumping();
        HandleCrouching();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        if (orientation == null)
        {
            Debug.LogError("Orientation is not assigned in PlayerMovement script!");
            return;
        }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.y = 0f;

        rb.linearDamping = drag;

        if (isGrounded && Input.GetKey(crouchKey) && Input.GetKey(sprintKey) && currentRunTime > 0)
        {
            moveSpeed = (crouchSpeed + defaultSpeed) / 2f;
        }
        else if (isGrounded && isRunning && !Input.GetKey(crouchKey) && currentRunTime > 0)
        {
            moveSpeed = runSpeed;
        }
        else if (isGrounded && Input.GetKey(crouchKey))
        {
            moveSpeed = crouchSpeed;
        }
        else
        {
            moveSpeed = defaultSpeed;
        }

        rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
    }

    private void HandleCrouching() // shrinks player down to half size when crouching
    {
        crouchSpeed = defaultSpeed / 2f;

        if (isGrounded && Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); // shrinks player size
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // pushes player down when the player shrinks
            moveSpeed = crouchSpeed;
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            moveSpeed = defaultSpeed;
        }
    }

    private void HandleRunning()
    {

        isRunning = Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift);

        if (isRunning && currentRunTime > 0) // if player wishes to run
        {
            currentRunTime -= Time.deltaTime;
        }
        else
        {
            moveSpeed = (currentRunTime <= 0) ? defaultSpeed / 2 : defaultSpeed;
            if (!isRunning && currentRunTime < maxRunTime)
            {
                currentRunTime += Time.deltaTime;
            }
        }

        currentRunTime = Mathf.Clamp(currentRunTime, 0, maxRunTime);
    }

    private void HandleJumping()
{
    isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer); // checks if player is on ground

    if (isGrounded && Input.GetKeyDown(jumpKey))
    {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // resets your y velocity to ensure consistent vertical speed

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z); // applies jumpforce
    }
}
    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * 9.81f, ForceMode.Acceleration);
        }
    }

}