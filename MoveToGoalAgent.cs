using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform goal;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private List<Transform> obstacles;
    [SerializeField] private GameObject obstacleHolder;
    [SerializeField] private Transform duplicateAgent;    

    private int totalObstacles = 0;
    private Rigidbody rb;

    public override void Initialize()
    {
        totalObstacles = obstacleHolder.transform.childCount;
        obstacles = new List<Transform>(totalObstacles);

        for (int i = 0; i < totalObstacles; i++)
        {
            obstacles.Add(obstacleHolder.transform.GetChild(i));
        }

        rb=GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Success Rate: " + GlobalVariables.sucessRate);
        Debug.Log("Failure Rate: " + GlobalVariables.failRate);
        Debug.Log("Total Episode: " + GlobalVariables.totalEpisodes);

        // Initialize random number generator
        System.Random random = new System.Random(100);

        for (int i = 0; i < totalObstacles ; i++)
        {
            float moveRotateY = UnityEngine.Random.Range(-45,45);
            obstacles[i].localPosition = new Vector3(UnityEngine.Random.Range(-14f + 6*i,-8f + 6*i), 0.5f, UnityEngine.Random.Range(-8f, 8f));
            obstacles[i].Rotate(0f, moveRotateY, 0f, Space.Self);
        }

        List<Tuple<int, int>> coordinates = new List<Tuple<int, int>>
        {
            new Tuple<int, int>(-14, 14),
            new Tuple<int, int>(14, -14)
        };

        // Select a random coordinate for the agent 
        int agentIndex = random.Next(coordinates.Count);
        var agentCoordinate = coordinates[agentIndex];
        coordinates.RemoveAt(agentIndex);

        // Select a random coordinate for the goal (from the updated list)
        var goalCoordinate = coordinates[random.Next(coordinates.Count)];

        // Calculate spawn positions
        Vector3 agentSpawnPos = new Vector3(UnityEngine.Random.Range(-8f, 8f), 0.5f, agentCoordinate.Item2);
        Vector3 goalSpawnPos = new Vector3(UnityEngine.Random.Range(-8f, 8f), 0.5f, goalCoordinate.Item2);

        // Set positions
        transform.localPosition = agentSpawnPos;
        goal.localPosition = goalSpawnPos;

        GlobalVariables.totalEpisodes += 1;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add Agent Position and Goal Position
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goal.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = actions.DiscreteActions[0];
        float moveRotateY = actions.DiscreteActions[1] - 1;
        float moveSpeed = 2f;
        
        Vector3 newPosition = transform.position + (transform.forward * moveForward * moveSpeed * Time.deltaTime);

        rb.MovePosition(newPosition);
        transform.Rotate(0f, moveRotateY * moveSpeed, 0f, Space.Self);

        // Time Penalty
        AddReward(-1f);

        if (Vector3.Distance(transform.localPosition, goal.localPosition) < 5f)
        {
            AddReward(0.2f);
        }

       if (Vector3.Distance(transform.localPosition, duplicateAgent.localPosition) < 2f)
        {
            AddReward(-0.2f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        // Reset actions
        discreteActions[0] = 0;
        discreteActions[1] = 1;

        // Map keys to actions
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActions[0] = 1; 
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActions[0] = 0; 
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[1] = 0; 
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[1] = 2; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goalComponent))
        {
            if(goalComponent.transform.localPosition == goal.localPosition)
            {
                AddReward(+10000f);
                GlobalVariables.sucessRate += 1;
                floorMeshRenderer.material = winMaterial;
                EndEpisode();
            }
            else
            {
                AddReward(-10000f);
                GlobalVariables.failRate += 1;
                floorMeshRenderer.material = loseMaterial;
                EndEpisode();
            }
        }
        else if (other.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-10000f);
            GlobalVariables.failRate += 1;
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.TryGetComponent<Obstacle>(out Obstacle obstacle))
        {
            AddReward(-5000f);
            GlobalVariables.failRate += 1;
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.TryGetComponent<Duplicate>(out Duplicate agent))
        {
            AddReward(-5000f);
            GlobalVariables.failRate += 1;
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.TryGetComponent<Duplicate1>(out Duplicate1 agentNew))
        {
            AddReward(-5000f);
            GlobalVariables.failRate += 1;
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
