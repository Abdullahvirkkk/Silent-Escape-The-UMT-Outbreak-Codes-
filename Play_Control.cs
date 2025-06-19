using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Health")]
    public float currentHealth;
    float maxHealth = 100f;
    private bool isDead = false;
    public Transform startPosition;
    public GameObject loadingScreen;

    [Header("Movement and Gravity")]
    public float speed = 5f;
    public float gravity = -9.81f;
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Camera")]
    public float mouseSensitivity = 2f;

    [Header("Jump and Crouch")]
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1f;
    public float crouchHeight = 0f;
    public float standHeight = 2f;
    public bool isCrouching = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentHealth = maxHealth;
    }

    void Update()
{
    isGrounded = characterController.isGrounded;

    if(isGrounded && velocity.y < 0)
    {
        velocity.y = -2f;
    }

    HandleCameraMovement();
    HandlePlayerMovement();

    velocity.y += gravity * Time.deltaTime;
    characterController.Move(velocity * Time.deltaTime);

    if(Input.GetKeyDown(KeyCode.Space))
    {
        HandleJump();
    }

    if(Input.GetKeyDown(KeyCode.C))
    {
        HandleCrouch();
    }
}

void HandlePlayerMovement()
{
    float currentSpeed = isCrouching ? crouchSpeed : speed;
    float moveForwardBackward = Input.GetAxis("Vertical") * currentSpeed;
    float moveLeftRight = Input.GetAxis("Horizontal") * currentSpeed;
    Vector3 move = transform.right * moveLeftRight + transform.forward * moveForwardBackward;
    characterController.Move(move * Time.deltaTime);
}

void HandleCameraMovement()
{
    float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
    float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
    
    transform.Rotate(Vector3.up * mouseX);
    
    float verticalLookRotation = Camera.main.transform.localEulerAngles.x - mouseY;
    Camera.main.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
}

public void HandleJump()
{
    if(isGrounded && ! isCrouching)
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}

public void HandleCrouch()
{
    isCrouching = !isCrouching;
    characterController.height = isCrouching ? crouchHeight : standHeight;
    characterController.radius = isCrouching ? 0.2f : 0.5f; 
}

public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if(currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

void Die()
{
    isDead = true;
    //decrease the day
    Game.instance.DecreaseDay(); 
    StartCoroutine(HandleRespawn());
}

IEnumerator HandleRespawn()
{
    yield return new WaitForSeconds(2f);

    //black screen
    loadingScreen.SetActive(true);

    characterController.enabled = false;
    transform.position = startPosition.position;
    characterController.enabled = true;

    yield return new WaitForSeconds(3f);

    //deactivate black screen 
    loadingScreen.SetActive(false);

    currentHealth = maxHealth;
    isDead = false;


}


}
