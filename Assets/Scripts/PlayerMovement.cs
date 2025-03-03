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
    
    public Transform cameraPivot;  // For the local camera
    public Transform spine;        // Spine bone to sync

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private CharacterController characterController;
    
    private PhotonView view;
    private Quaternion syncedSpineRotation;

    void Start() {
        characterController = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();

        // Disable camera for remote players
        if (!view.IsMine && cameraPivot != null) {
            cameraPivot.gameObject.SetActive(false);
        } 
        else {
            // Local player setup
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update() {
        if (view.IsMine) {
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            if (cameraPivot != null) {
                cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
            }

            if (spine != null && cameraPivot != null) {
                spine.rotation = cameraPivot.rotation;
            }
            
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right   = transform.TransformDirection(Vector3.right);

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
        }
        else {
            if (spine != null) {
                spine.rotation = Quaternion.Lerp(spine.rotation, syncedSpineRotation, Time.deltaTime * 10f);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(spine.rotation);
        } else {
            syncedSpineRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
