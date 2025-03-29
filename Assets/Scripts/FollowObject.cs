using UnityEngine;

public class FollowObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Transform Target;

    public Vector2 Offset;

    void Start()
    {
       
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Target == null)
        {
            return;
        }

        transform.position = new Vector3(Target.position.x + Offset.x, Target.position.y + Offset.y, transform.position.z);
    }
}
