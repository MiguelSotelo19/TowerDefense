using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(FollowPathAgent))]
public class ExtraPath : MonoBehaviour
{
    [Header("Spline que seguirá este objeto")]
    public SplineContainer splineToFollow;

    private FollowPathAgent pathAgent;

    private void Awake()
    {
        pathAgent = GetComponent<FollowPathAgent>();
    }

    private void Start()
    {
        if (splineToFollow == null)
        {
            return;
        }

        pathAgent.AssignSplineContainer(splineToFollow);
        pathAgent.ResetProgress(keepWorldPosition: false);
        pathAgent.enabled = true;
    }

    public void SetSpline(SplineContainer newSpline)
    {
        splineToFollow = newSpline;

        if (pathAgent == null)
            pathAgent = GetComponent<FollowPathAgent>();

        if (newSpline != null)
        {
            pathAgent.AssignSplineContainer(newSpline);
            pathAgent.ResetProgress(keepWorldPosition: false);
        }
    }
}
