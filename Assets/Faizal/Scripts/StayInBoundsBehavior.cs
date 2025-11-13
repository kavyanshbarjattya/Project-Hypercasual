using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(SteeringAgent))]
public class StayInBoundsBehavior : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Bounds generationBounds;
    public float boundaryTurnStrength = 2f;

    private SteeringAgent agent;

    void Start()
    {
        agent = GetComponent<SteeringAgent>();
        // Start the agent in the middle of the box
        agent.currentPosition = generationBounds.center;
    }

    /// <summary>
    /// This function is called by the SteeringAgent's "SendMessage"
    /// </summary>
    void CalculateSteering()
    {
        float3 turnForce = float3.zero;
        float3 headPos = agent.currentPosition; // Get agent's position

        // Check X
        if (headPos.x > generationBounds.max.x) turnForce.x = -1f;
        else if (headPos.x < generationBounds.min.x) turnForce.x = 1f;

        // Check Y
        if (headPos.y > generationBounds.max.y) turnForce.y = -1f;
        else if (headPos.y < generationBounds.min.y) turnForce.y = 1f;

        // Check Z
        if (headPos.z > generationBounds.max.z) turnForce.z = -1f;
        else if (headPos.z < generationBounds.min.z) turnForce.z = 1f;

        // Send this force to the agent, scaled by our settings
        agent.AddForce(turnForce * boundaryTurnStrength * Time.deltaTime);
    }

    // Draw the gizmo for visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(generationBounds.center, generationBounds.size);
    }
}