using UnityEngine;
using Unity.Mathematics;

public class SteeringAgent : MonoBehaviour
{
    [Header("Agent Stats")]
    public float moveSpeed = 10f;

    // This is the "state" of the agent
    [HideInInspector] public float3 currentPosition;
    [HideInInspector] public float3 currentDirection;

    private float3 steeringForce;

    void Start()
    {
        currentPosition = transform.position;
        currentDirection = transform.forward;
    }

    void Update()
    {
        // 1. Reset steering force from last frame
        steeringForce = float3.zero;

        // 2. Broadcast for behaviors to add their forces
        // (Other scripts will add to our 'steeringForce' variable)
        SendMessage("CalculateSteering", SendMessageOptions.DontRequireReceiver);

        // 3. Apply the steering to our direction
        currentDirection = currentDirection + steeringForce;
        currentDirection = math.normalize(currentDirection);

        // 4. Move the agent
        currentPosition += currentDirection * moveSpeed * Time.deltaTime;

        // 5. Update the GameObject's transform
        transform.position = currentPosition;
    }

    /// <summary>
    /// This is a public "inbox" for other scripts
    /// to add their steering forces.
    /// </summary>
    public void AddForce(float3 force)
    {
        steeringForce += force;
    }
}