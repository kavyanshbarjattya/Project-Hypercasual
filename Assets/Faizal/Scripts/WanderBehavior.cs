using UnityEngine;
using Unity.Mathematics;
[RequireComponent(typeof(SteeringAgent))]
public class WanderBehavior : MonoBehaviour
{
    [Header("Wander Settings")]
    public float turnStrength = 45f;
    public float noiseFrequency = 0.1f;

    private SteeringAgent agent;
    private float perlinSeedX;
    private float perlinSeedY;

    void Start()
    {
        agent = GetComponent<SteeringAgent>();
        perlinSeedX = UnityEngine.Random.Range(0f, 1000f);
        perlinSeedY = UnityEngine.Random.Range(0f, 1000f);
    }

    /// <summary>
    /// This function is called by the SteeringAgent's "SendMessage"
    /// </summary>
    void CalculateSteering()
    {
        // 1. Get Perlin noise values
        float noiseSampleX = (Time.time * noiseFrequency) + perlinSeedX;
        float noiseSampleY = (Time.time * noiseFrequency) + perlinSeedY;

        float turnAngleX = (Mathf.PerlinNoise(noiseSampleX, 0) - 0.5f) * 2f;
        float turnAngleY = (Mathf.PerlinNoise(noiseSampleY, 0) - 0.5f) * 2f;

        // 2. Create the turn rotation
        Quaternion turnRotation = Quaternion.Euler(
            turnAngleY * turnStrength * Time.deltaTime, 
            turnAngleX * turnStrength * Time.deltaTime, 
            0);
        
        // 3. Calculate the "force" (the change in direction)
        float3 wanderForce = (float3)(turnRotation * agent.currentDirection) - agent.currentDirection;

        // 4. Send this force to the agent
        agent.AddForce(wanderForce);
    }
}