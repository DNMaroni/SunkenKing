using UnityEngine;

public class CameraFollow : MonoBehaviour {
    
    public Transform target;

    void LateUpdate() {
        transform.position =  new Vector3(target.position.x, transform.position.y, transform.position.z);
    }
}
