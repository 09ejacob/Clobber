using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IPunObservable {
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 9.81f;
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;
    public float defaultHeight = 0.2f;
    public float crouchHeight = 0f;
    public float crouchSpeed = 3f;
    
    public Transform cameraPivot;   // This is the parent for your camera (only active for local player)
    public Transform spine;         // Reference to the spine bone you want to sync

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private CharacterController characterController;
    
    PhotonView view;
    
    // Used to store the spine rotation received from the network
    private Quaternion syncedSpineRotation;

    void Start() {
        characterController = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();

        if (!view.IsMine) {
            // Disable the local camera for remote players
            if (cameraPivot != null) {
                cameraPivot.gameObject.SetActive(false);
            }
        } else {
            // Lock cursor for local player
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update() {
        if (view.IsMine) {
            // Horizontal rotation rotates the player object (yaw)
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            // Vertical rotation rotates the camera pivot (pitch)
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            if (cameraPivot != null) {
                cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
            }
            
            // Update the spine rotation to follow the camera pivot.
            // You can adjust this logic if you need an offset or a different behavior.
            if (spine != null && cameraPivot != null) {
                spine.rotation = cameraPivot.rotation;
            }
            
            // Movement, jump, crouch, etc.
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float curSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
            float curSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButton("Jump") && characterController.isGrounded) {
                moveDirection.y = jumpPower;
            } else {
                moveDirection.y = movementDirectionY;
            }

            if (!characterController.isGrounded) {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.LeftControl)) {
                characterController.height = crouchHeight;
            } else {
                characterController.height = defaultHeight;
            }

            characterController.Move(moveDirection * Time.deltaTime);
        } else {
            // For remote players, smoothly update the spine rotation with the synced value.
            if (spine != null) {
                spine.rotation = Quaternion.Lerp(spine.rotation, syncedSpineRotation, Time.deltaTime * 10f);
            }
        }
    }

    // This method is called by Photon to synchronize data.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // Send the spine rotation from the local player
            stream.SendNext(spine.rotation);
        } else {
            // Receive and store the spine rotation for remote players
            syncedSpineRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
