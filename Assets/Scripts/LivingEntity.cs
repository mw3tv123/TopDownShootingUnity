using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {
    public float StartHealthPoint;

    protected float HealthPoint;
    protected bool Dead;

    protected Material SkinMaterial;
    protected Color OriginalColor;

    public event Action OnDeath;

    protected virtual void Awake () {
        HealthPoint = StartHealthPoint;
        SkinMaterial = GetComponent<Renderer> ().material;
        OriginalColor = SkinMaterial.color;
    }

    public void GetHit (float amount) {
        SkinMaterial.color = Color.red; ;
        HealthPoint -= amount;
        if (HealthPoint <= 0 && !Dead) {
            Die ();
        }
        SkinMaterial.color = OriginalColor;
    }

    public virtual void GetHit (float amount, Vector3 hitPoint, Vector3 direction) {
        GetHit (amount);
    }

    [ContextMenu("Self Destruct")]
    protected void Die () {
        Dead = true;
        OnDeath?.Invoke ();
        Destroy (gameObject);
    }
}
