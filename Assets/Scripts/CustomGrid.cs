using UnityEngine;

[ExecuteInEditMode]
public class CustomGrid : MonoBehaviour
{
    public GameObject target;
    public GameObject structure;
    public float gridSize;

    private Vector3 truePos;

    // Update is called once per frame
    private void LateUpdate()
    {
        truePos.x = Mathf.Round(target.transform.position.x / gridSize) * gridSize;
        truePos.y = 0;
        truePos.z = Mathf.Round(target.transform.position.z / gridSize) * gridSize;

        structure.transform.position = truePos;
    }
}
