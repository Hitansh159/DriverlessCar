using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAI : Agent
{

    CarController controller;
    Rigidbody rb;
    
    List<GameObject> checkpoints;
    int nextCheckpointIndex = 0;
    public GameObject checkpointsParent;
    CheckpointManager checkpointManager;

    float health = 100;
    public Transform startPoint;
    
    public float reward = 0.0f;

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        //Getting Car controll
        controller = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        //spawning on random position
        transform.position = startPoint.position + RandomOffset(0.1f, -0.1f);
        transform.rotation = startPoint.rotation;

        //Some vitals
        health = 100;
        reward = 0.0f;

        // checkpoint details 
        checkpointManager = checkpointsParent.GetComponent<CheckpointManager>();
        nextCheckpointIndex = 0;
        checkpoints = new List<GameObject>();
        for(int i=0;i<checkpointsParent.transform.childCount;i++)
        {
            Debug.Log(checkpointsParent.transform.GetChild(i).gameObject.name);
            checkpoints.Add(checkpointsParent.transform.GetChild(i).gameObject);
        }
        Debug.Log("Number of children "+ checkpoints.Count);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmmount = 0f;
        float turnAmmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmmount = 1f; AddReward(0.01f); reward += 0.01f; break;
            case 1: forwardAmmount = 0.5f; break;
            case 2: forwardAmmount = 0.2f;break;
        }
        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmmount = 1f;break;
            case 1: turnAmmount = -1f;break;
            case 2: turnAmmount = 0f;break;
        }

        controller.Drive(forwardAmmount, turnAmmount);
        
        if (health < 0)
            EndEpisode();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("Collided with wall");
            AddReward(-1.0f);
            reward -= 1f;
            health -= 20f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("stuck with wall");
            AddReward(-2.0f);
            reward -= 2f;
            health -= 2f;
        }
    }

    public void CheckpointPass(Checkpoint other)
    {
        if(other.gameObject.Equals(checkpoints[nextCheckpointIndex]))
        {
            AddReward(1.0f);
            reward += 1;
            /*float passReward = checkpointManager.CheckpointCrossReward(transform.position, nextCheckpointIndex);
            AddReward(passReward);
            reward += passReward;*/
            AddReward(rb.velocity.magnitude);
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Count;
        }
        else
        {
            AddReward(-10.0f);
            reward -= 10f;
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetAxis("Vertical") < 0 ? 1: Input.GetAxis("Vertical") > 0 ? 0: 2;
        discreteActions[1] = Input.GetAxis("Horizontal") < 0 ? 1 : Input.GetAxis("Horizontal") > 0 ? 0 : 2;
    }

    private Vector3 RandomOffset(float max, float min)
    {
        return new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
    }

    void OnDrawGizmosSelected()
    {
        if (checkpointManager.walls[nextCheckpointIndex] != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, checkpointManager.CheckpointCenter(nextCheckpointIndex));
        }
    }

}
