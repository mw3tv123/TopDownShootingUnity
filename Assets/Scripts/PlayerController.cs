using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {
    private Rigidbody _rigidbody;
    private Vector3 _velocity;

    // Start is called before the first frame update
    private void Start () {
        _rigidbody = GetComponent<Rigidbody> ();
    }

    private void FixedUpdate () {
        _rigidbody.MovePosition (_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    public void Move (Vector3 velocity) {
        _velocity = velocity;
    }

    public void LookAt (Vector3 point) {
        transform.LookAt (new Vector3 (point.x, transform.position.y, point.z));
    }
}
