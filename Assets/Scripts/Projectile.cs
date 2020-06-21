using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField]
    private float _speed = 10f;
    public float Speed {
        get => _speed;
        set => _speed = value;
    }

    [SerializeField]
    private float damage = 1f;

    private const float LifeTime = 5f;

    private void Start () {
        Destroy (gameObject, LifeTime);
    }

    // Update is called once per frame
    private void Update() {
        transform.Translate (Vector3.forward * Time.deltaTime * Speed);
    }

    private void OnTriggerEnter (Collider otherObject) {
        var damageableObject = otherObject.GetComponent<IDamageable> ();
        damageableObject?.GetHit (
            damage,
            otherObject.ClosestPointOnBounds (transform.position),
            transform.forward
        );
        Destroy (gameObject);
    }

    private void OnCollisionEnter (Collision other) {
        Destroy (gameObject);
    }
}
