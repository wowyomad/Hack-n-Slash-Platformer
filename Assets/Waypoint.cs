using UnityEngine;

public class Waypoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    static int WaypointCounter = 0;

    protected void Start()
    {
        WaypointCounter++;
    }
}
