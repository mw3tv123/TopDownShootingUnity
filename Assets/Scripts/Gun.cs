using UnityEngine;

public class Gun : MonoBehaviour {
    public enum FireMode {
        Auto,
        Burst,
        Single,
    }

    public Transform Muzzle;
    public Transform BulletEjectionHole;
    public Projectile Bullet;
    public BulletCase BulletCase;
    public ParticleSystem MuzleFlare;
    public GameObject Light;
    public GameObject GunSound;
    public float TimeDelayBetweenShoot = 100f;
    public float InitialVelocity = 35f;
    public FireMode Mode;

    private const int BurstCount = 3;
    private float timeFromLastShot;
    private bool isTriggerRelease;
    private int shotsFired;

    private void Shoot () {
        if (Time.time > timeFromLastShot) {
            if (Mode is FireMode.Burst) {
                if (shotsFired == BurstCount) {
                    return;
                }
                shotsFired++;
            }
            else if (Mode is FireMode.Single && !isTriggerRelease) {
                return;
            }
            timeFromLastShot = Time.time + TimeDelayBetweenShoot / 1000;

            // Create gun sound effect
            Destroy (Instantiate (GunSound, Muzzle.position, Muzzle.rotation), 1f);
            // Create bullet at gun muzzle
            Projectile newBullet = Instantiate (Bullet, Muzzle.position, Muzzle.rotation);
            newBullet.Speed = InitialVelocity;
            // Create muzzle flare
            Destroy (Instantiate (MuzleFlare, Muzzle.position, Muzzle.rotation).gameObject, MuzleFlare.main.startLifetime.constant);
            Light.SetActive (true);
            Invoke ("Deactivate", 0.05f);
            
            // Create bullet case at bullet ejection hole
            Instantiate (BulletCase, BulletEjectionHole.position, BulletEjectionHole.rotation);
        }
    }

    private void Deactivate () => Light.SetActive (false);

    public void OnTriggerHold () {
        Shoot ();
        isTriggerRelease = false; 
    }

    public void OnTriggerRelease () {
        isTriggerRelease = true;
        shotsFired = 0;
    }
}
