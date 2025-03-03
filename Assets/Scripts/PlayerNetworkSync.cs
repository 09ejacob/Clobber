using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSync : MonoBehaviour, IPunObservable {
    public Transform spine;         // Assign this in the Inspector
    public Transform cameraPivot;   // Needed to get the local pitch (X rotation)

    private Vector3 syncedPosition;
    private float syncedPlayerYRotation;
    private float syncedSpinePitch; // The cameraPivot's local X (pitch)

    void Update() {
        PhotonView view = GetComponent<PhotonView>();
        if (!view.IsMine) {
            // Smoothly interpolate position
            float distance = Vector3.Distance(transform.position, syncedPosition);
            if (distance > 3f) {
                transform.position = syncedPosition;
            } else {
                transform.position = Vector3.Lerp(transform.position, syncedPosition, Time.deltaTime * 10f);
            }
            
            // Interpolate player's Y rotation (yaw)
            float newY = Mathf.LerpAngle(transform.eulerAngles.y, syncedPlayerYRotation, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Euler(0, newY, 0);
            
            // Interpolate the spine's local X rotation (pitch)
            if (spine != null) {
                Quaternion targetLocal = Quaternion.Euler(syncedSpinePitch, 0, 0);
                spine.localRotation = Quaternion.Lerp(spine.localRotation, targetLocal, Time.deltaTime * 10f);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // Local player sends:
            stream.SendNext(transform.position);
            stream.SendNext(transform.eulerAngles.y);
            stream.SendNext(cameraPivot != null ? cameraPivot.localEulerAngles.x : 0f);
        } else {
            // Remote players receive:
            syncedPosition = (Vector3)stream.ReceiveNext();
            syncedPlayerYRotation = (float)stream.ReceiveNext();
            syncedSpinePitch = (float)stream.ReceiveNext();
        }
    }
}
