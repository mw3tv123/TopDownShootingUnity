using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity {
    public enum  State {
        Idle,
        Chasing,
        Attacking
    };

    public ParticleSystem DeathEffect;

    private NavMeshAgent agent;
    private GameObject target;
    private State currentState;
    private LivingEntity targetEntity;

    [SerializeField]
    private float damage = 1f;
    [SerializeField]
    private float attackRange = 1.5f;
    [SerializeField]
    private float attackSpeed = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;
    private float timeSinceLastAttack;
    private float myCollisionRadius;
    private float targetCollisionRadius;

    private bool hasTarget;

    protected override void Awake () {
        base.Awake ();
        agent = GetComponent<NavMeshAgent> ();
    }

    // Start is called before the first frame update
    private void Start () {
        if (GameObject.FindGameObjectWithTag ("Player") != null) {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag ("Player");
            targetEntity = target.GetComponent<LivingEntity> ();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;

            StartCoroutine (UpdatePath ());
        }
    }

    protected void Update () {
        if (hasTarget && Time.time > timeSinceLastAttack) {
            float sqrDistanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
            if (sqrDistanceToTarget < Mathf.Pow (attackRange + myCollisionRadius + targetCollisionRadius, 2)) {
                timeSinceLastAttack = Time.time + timeBetweenAttacks;
                StartCoroutine (Attack ());
            }
        }
    }

    private void OnTargetDeath () {
        hasTarget = false;
        currentState = State.Idle;
    }

    private IEnumerator Attack() {
        currentState = State.Attacking;
        agent.enabled = false;

        // Simulate attack animation
        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
        Vector3 attackPosition = target.transform.position - dirToTarget * myCollisionRadius;
        float percent = 0;
        bool hasAppliedDamage = false;
        while (percent <= 1) {
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);

            if (percent >= 0.5f && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.GetHit (damage);
            }

            yield return null;
        }

        currentState = State.Chasing;
        agent.enabled = true;
    }

    private IEnumerator UpdatePath () {
        const float refrestTime = 0.25f; // A quarter of second.

        while (hasTarget) {
            if (currentState is State.Chasing && !Dead) {
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                Vector3 targetPosition = target.transform.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackRange / 2);
                agent.SetDestination (targetPosition);
            }
            yield return new WaitForSeconds (refrestTime);
        }
    }

    public override void GetHit (float amount, Vector3 hitPoint, Vector3 direction) {
        if (amount >= HealthPoint) {
            Destroy (
                Instantiate (
                    DeathEffect.gameObject,
                    hitPoint,
                    Quaternion.FromToRotation (Vector3.forward, direction)
                    ),
                DeathEffect.main.startLifetime.constant
                );
        }
        base.GetHit (amount, hitPoint, direction);
    }

    public void SetCharacteristics (float damageModifier, float healthModifier, float moveSpeedModifier, Color skinColor) {
        agent.speed *= moveSpeedModifier;
        damage *= damageModifier;
        HealthPoint *= healthModifier;
        SkinMaterial.color = skinColor;
        OriginalColor = SkinMaterial.color;
    }
}
