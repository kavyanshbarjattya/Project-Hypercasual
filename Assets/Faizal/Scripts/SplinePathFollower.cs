using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class SplinePathFollower : MonoBehaviour
{
    [Header("Spline Settings")]
    public SplineContainer splineContainer;
    public Transform agentToFollow; // <-- DRAG your "PathHead" here

    [Header("Path Settings")]
    public int pathLengthInKnots = 50;
    public float segmentLength = 2f; // How far to move before dropping a knot

    private float3 lastKnotPosition;

    void Start()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        splineContainer.Spline.Clear();

        // Set the starting position
        lastKnotPosition = agentToFollow.position;

        // Create an initial knot
        splineContainer.Spline.Add(new BezierKnot(lastKnotPosition), TangentMode.AutoSmooth);
    }

    void Update()
    {
        float3 agentPos = agentToFollow.position;

        // Check if the agent has moved far enough
        if (math.distance(agentPos, lastKnotPosition) >= segmentLength)
        {
            // Add a new knot at the agent's current position
            splineContainer.Spline.Add(new BezierKnot(agentPos), TangentMode.AutoSmooth);
            lastKnotPosition = agentPos;

            // Remove the tail knot if the spline is too long
            if (splineContainer.Spline.Count > pathLengthInKnots)
            {
                splineContainer.Spline.RemoveAt(0);
            }
        }
    }
}
