using UnityEngine;

public interface IDamageable {
    void GetHit (float amount);

    void GetHit (float amount, Vector3 hitPoint, Vector3 direction);
}
