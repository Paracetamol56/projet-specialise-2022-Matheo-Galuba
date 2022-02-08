using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    // Agent vision : the position of the target
    [SerializeField]
    private Transform target;
    // Move speed
    [SerializeField]
    private float moveSpeed = 5f;

    public override void OnEpisodeBegin()
    {
        // Reset the agent position
        transform.localPosition = new Vector3(0f, 1f, 0f);
        // Reset the target position
        target.localPosition = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
    }

    // Method called when the neural network request observation data
    public override void CollectObservations(VectorSensor sensor)
    {
        // Lunch 3 raycast in fornt of the agent with an angle of 30°
        // Make a layer mask to ignore the "Environment" layer
        int environmentLayer = LayerMask.NameToLayer("Environment");
        int layerMask = 1 << environmentLayer;
        // Eyes position
        Vector3 raycastOrigin = transform.localPosition + Vector3.forward * 0.6f;
        // Central raycast
        Vector3 raycastDirection = new Vector3(0f, 0f, 0f);
        Physics.Raycast(raycastOrigin, raycastDirection, out RaycastHit hit, 10.0f);
        Debug.DrawRay(raycastOrigin, raycastDirection, hit.collider ? Color.green : Color.red);
        sensor.AddObservation(hit.distance);
        Debug.Log(hit.distance);

        // Left raycast
        raycastDirection = Quaternion.Euler(0f, 30f, 0f) * new Vector3(0f, 0f, 0f);
        Physics.Raycast(raycastOrigin, raycastDirection, out hit, 10.0f);
        Debug.DrawRay(raycastOrigin, raycastDirection, hit.collider ? Color.green : Color.red);
        sensor.AddObservation(hit.distance);
        Debug.Log(hit.distance);

        // Right raycast
        raycastDirection = Quaternion.Euler(0f, -30f, 0f) * new Vector3(0f, 0f, 0f);
        Physics.Raycast(raycastOrigin, raycastDirection, out hit, 10.0f);
        Debug.DrawRay(raycastOrigin, raycastDirection, hit.collider ? Color.green : Color.red);
        sensor.AddObservation(hit.distance);
        Debug.Log(hit.distance);
    }

    // Method called when the agent receive data from the neural network
    public override void OnActionReceived(ActionBuffers actions)
    {
        // === Uncomment to debug the agent === //
        // Debug.Log("OnActionReceived");
        // This is to see the output of the neural network with continuous actions
        // Debug.Log("Action: " + actions.ContinuousActions[0]);
        // This is to see the output of the neural network with discrete actions
        // Debug.Log("Action: " + actions.DiscreteActions[0]);

        // Recieve the action from the neural network
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        // Move the agent
        transform.Translate(0f, 0f, moveY * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveX * 180f * Time.deltaTime, 0f);
    }

    // Methode called when the user override the agent actions with heuristic mode
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    // Method called when the agent reach the goal
    private void OnTriggerStay(Collider other)
    {
        // === Uncomment to debug the agent === //
        // Debug.Log("OnTriggerEnter");

        // If the agent touch the target
        if (other.CompareTag("goal"))
        {
            // === Uncomment to debug the agent === //
            // Debug.Log("Goal reached");

            // Reward the agent
            AddReward(1f);
            // End the episode
            EndEpisode();
        }

        // If the agent touch the wall
        if (other.CompareTag("wall"))
        {
            // === Uncomment to debug the agent === //
            // Debug.Log("Wall touched");

            // Punish the agent
            AddReward(-1f);
            // End the episode
            EndEpisode();
        }
    }
}
