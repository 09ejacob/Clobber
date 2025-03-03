using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpPower = 4f;
    public float gravity = 9.81f;
    public float crouchSpeed = 1.5f;

    [Header("Rotation Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;
    public float defaultHeight = 1f;
    public float crouchHeight = 0f;

    [Header("References")]
    public Transform cameraPivot;  // For local camera control
    public Transform spine;        // Spine bone to sync with camera rotation

    private CharacterController characterController;
    private float rotationX = 0f;
    private Vector3 moveDirection = Vector3.zero;
    private PhotonView view;

    void Start() {
        characterController = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();

        if (!view.IsMine) {
            // Disable physics simulation for remote players
            characterController.enabled = false;
            if (cameraPivot != null)
                cameraPivot.gameObject.SetActive(false);
        } else {
            // Setup for local player
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update() {
        if (view.IsMine)
            ProcessInput();
    }

    void ProcessInput() {
        // --- Rotation ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Yaw rotates the whole player object
        transform.Rotate(0, mouseX * lookSpeed, 0);

        // Pitch rotates the camera pivot
        rotationX -= mouseY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Set the spine rotation to match the camera pivot
        if (spine != null && cameraPivot != null)
            spine.rotation = cameraPivot.rotation;

        // --- Movement ---
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");

        float movementDirectionY = moveDirection.y; // Preserve vertical velocity (for gravity/jump)
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping
        if (Input.GetButton("Jump") && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        // Apply gravity when not grounded
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Adjust height for crouching
        if (Input.GetKey(KeyCode.LeftControl))
            characterController.height = crouchHeight;
        else
            characterController.height = defaultHeight;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
