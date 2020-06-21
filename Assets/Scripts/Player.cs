using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {
    public float MoveSpeed = 5;

    private PlayerController playerController;
    private GunController gunController;
    private Camera viewCamera;

    // Start is called before the first frame update
    protected override void Awake () {
        base.Awake ();

        playerController = GetComponent<PlayerController> ();
        gunController = GetComponent<GunController> ();
        viewCamera = Camera.main;
    }

    // Update is called once per frame
    private void Update () {
        // Movement Input
        var moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
        var moveVelocity = moveInput.normalized * MoveSpeed;
        playerController.Move (moveVelocity);

        // Look Input
        var ray = viewCamera.ScreenPointToRay (Input.mousePosition);
        var groundPlane = new Plane (Vector3.up, Vector3.zero);

        if (groundPlane.Raycast (ray, out var rayDistance)) {
            var point = ray.GetPoint (rayDistance);
            Debug.DrawLine (ray.origin, point, Color.red);
            playerController.LookAt (point);
        }

        // Weapon Input
        if (Input.GetMouseButton (0)) {
            gunController.OnTriggerHold ();
        }
        if (Input.GetMouseButtonUp (0)) {
            gunController.OnTriggerRelease ();
        }
    }
}
