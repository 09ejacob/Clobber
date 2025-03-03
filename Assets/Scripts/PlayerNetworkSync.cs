using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSync : MonoBehaviour, IPunObservable {
    public Transform spine; // Assign this in the Inspector

    private Vector3 syncedPosition;
    private Quaternion syncedSpineRotation;

    void Update() {
        PhotonView view = GetComponent<PhotonView>();
        if (!view.IsMine) {
            // If the distance is too big, snap to the target position
            float distance = Vector3.Distance(transform.position, syncedPosition);
            if (distance > 3f) {
                transform.position = syncedPosition;
            } else {
                // Otherwise, smoothly interpolate the position
                transform.position = Vector3.Lerp(transform.position, syncedPosition, Time.deltaTime * 10f);
            }
            
            // Smoothly interpolate the spine rotation
            if (spine != null) {
                spine.rotation = Quaternion.Lerp(spine.rotation, syncedSpineRotation, Time.deltaTime * 10f);
            }
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // Send the local player's position and spine rotation
            stream.SendNext(transform.position);
            if (spine != null)
                stream.SendNext(spine.rotation);
        } else {
            // Receive and store the networked position and spine rotation
            syncedPosition = (Vector3)stream.ReceiveNext();
            syncedSpineRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
