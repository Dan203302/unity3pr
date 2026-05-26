using UnityEngine;

public class DynamicObstacle : MonoBehaviour
{
    public float moveDistance = 3f;
    public float speed = 1.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveDistance;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}
