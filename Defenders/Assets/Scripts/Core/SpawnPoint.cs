using UnityEngine;
using UnityEngine.Splines;

public class SpawnPoint : MonoBehaviour
{

    public int spawnID = 0;
    public SplineContainer associatedSpline;

    public Color spawnColor = Color.red;

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    
}
