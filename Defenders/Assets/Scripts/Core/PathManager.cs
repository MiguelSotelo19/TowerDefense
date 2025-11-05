using UnityEngine;

public class PathManager : MonoBehaviour
{
    
    public Transform[] waypoints; // Waypoints = camino
    public Color pathColor = Color.cyan;
    public float waypointRadius = 0.5f; //radio esferas

    public Vector3 GetWaypointPosition(int index)
    {
        if (index < 0 || index >= waypoints.Length)
            return Vector3.zero;
        return waypoints[index].position;
    }

    public int GetWaypointCount()
    {
        return waypoints.Length;
    }

    void OnDrawGizmos() //Para ver en el editor
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Dibujar gizmo
        Gizmos.color = pathColor;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Dibujar esferas en cada waypoint
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawWireSphere(waypoint.position, waypointRadius);
                Gizmos.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.3f);
                Gizmos.DrawSphere(waypoint.position, waypointRadius);
            }
        }
    }
}
