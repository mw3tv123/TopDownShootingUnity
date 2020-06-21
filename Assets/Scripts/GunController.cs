using UnityEngine;

public class GunController : MonoBehaviour {
    public Transform WeaponHold;
    public Gun StartingGun;

    private Gun currentGun;

    private void Start () {
        if (StartingGun != null) {
            EquipGun (StartingGun);
        }
    }

    public void EquipGun (Gun newGun) {
        if (currentGun != null) {
            Destroy (currentGun.gameObject);
        }
        currentGun = Instantiate (newGun, WeaponHold.position, WeaponHold.rotation);
        currentGun.transform.parent = WeaponHold;
    }

    public void OnTriggerHold () {
        if (currentGun != null) {
            currentGun.OnTriggerHold ();
        }
    }

    public void OnTriggerRelease () {
        if (currentGun != null) {
            currentGun.OnTriggerRelease ();
        }
    }
}
