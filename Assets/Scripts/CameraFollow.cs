using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0, 4, -7);
    [SerializeField] float smoothTime = 0.3f, lookAhead = 0.35f;   // seconds
    Rigidbody targetBody;
    Vector3 velocity;                              // SmoothDamp's memory

    void Awake()
    {
        if (target == null)
            target = FindAnyObjectByType<PlayerController>().transform;
        targetBody = target.GetComponent<Rigidbody>();
        transform.position = target.position + offset;   // start in place, no swoop
    }

    void LateUpdate()
    {
        Vector3 v = targetBody.linearVelocity;
        Vector3 ahead = new Vector3(v.x, 0f, v.z) * lookAhead;
        Vector3 wanted = target.position + offset + ahead;
        transform.position = Vector3.SmoothDamp(transform.position, wanted,
                                                ref velocity, smoothTime);
        transform.LookAt(transform.position - offset);   // the aim point, not the player
    }
}
