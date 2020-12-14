using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayCollabAgent : HallwayAgent
{
    public HallwayCollabAgent teammate;
    public bool isSpotter = true;
    int m_Message = 0;
    public override void OnEpisodeBegin()
    {
        // Set initial message to random to avoid initialization issues
        m_Message = Random.Range(0, 2);

        var agentOffset = 15f;
        if (isSpotter)
        {
            agentOffset = -15f;
        }

        transform.position = new Vector3(0f + Random.Range(-3f, 3f),
            1f, agentOffset + Random.Range(-5f, 5f))
            + ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;
        if (isSpotter)
        {
            var blockOffset = 0f;
            m_Selection = Random.Range(0, 2);
            if (m_Selection == 0)
            {
                symbolO.transform.position =
                    new Vector3(0f + Random.Range(-3f, 3f), 2f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
            }
            else
            {
                symbolO.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, 2f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
            }

            var goalPos = Random.Range(0, 2);
            if (goalPos == 0)
            {
                symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }
            else
            {
                symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
        sensor.AddObservation(m_Message);
    }

    public void tellAgent(int message)
    {
        m_Message = message;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
        int comm_act = actionBuffers.DiscreteActions[1];
        teammate.tellAgent(comm_act);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if ((m_Selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                (m_Selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
            {
                SetReward(1f);
                teammate.SetReward(1f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            }
            else
            {
                SetReward(-0.1f);
                teammate.SetReward(-0.1f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
            }
            EndEpisode();
            teammate.EndEpisode();
        }
    }
}