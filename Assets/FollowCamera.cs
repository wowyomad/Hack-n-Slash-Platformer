using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Transform Target;
    void Start()
    {
        if (Target == null)
        {
            Target = GameObject.FindWithTag("Player")?.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Target == null)
        {
            return;
        }

        transform.position = new Vector3(Target.position.x, Target.position.y, transform.position.z);
    }
}
