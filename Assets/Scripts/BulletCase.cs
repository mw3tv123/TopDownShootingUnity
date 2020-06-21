using System.Collections;
using UnityEngine;

public class BulletCase : MonoBehaviour {
    public Rigidbody Rigidbody;
    public float ForceMin;
    public float ForceMax;

    private const float LifeTime = 4f;

    // Start is called before the first frame update
    private void Start () {
        float force = Random.Range (ForceMin, ForceMax);
        Rigidbody.AddForce (transform.right * force);
        Rigidbody.AddTorque (Random.insideUnitSphere * force);
        StartCoroutine (Fade ());
    }

    private IEnumerator Fade () {
        yield return new WaitForSeconds (LifeTime);

        float percent = 0;
        float fadeSpeed = 1 / percent;
        Material material = GetComponentInChildren<Renderer>().material;
        Color originalColor = material.color;
        while (percent < 1) {
            percent += Time.deltaTime * fadeSpeed;
            material.color = Color.Lerp (originalColor, Color.clear, percent);
            yield return null;
        }
        Destroy (gameObject);
    }
}
