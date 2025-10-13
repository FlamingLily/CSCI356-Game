using UnityEngine;

public class MoveUpDown : MonoBehaviour
{
    public float speed = 2f;        
    public float height = 2f;        
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {

        float newY = startPos.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
