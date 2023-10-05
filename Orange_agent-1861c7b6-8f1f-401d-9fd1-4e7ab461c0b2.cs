using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class Orange_agent : Agent
{
    //all orange cubes data
    public List<Transform> orangeCubesTransforms;

    //message data
    public TextMeshProUGUI messageText;

    public Renderer orangeAgentTurret;

    //materials used for better visualization
    public Material normalMaterial;
    public Material messageMaterial;

    //distance to a nearby cube
    public float distanceThreshold = 2.6f;
    public float maxDistanceThreshold = 1.2f;


    //AI agent data
    public Transform mainAgentTransform;


    //finds the orange cubes in the environment
    private void Awake()
    {
        GameObject[] orangeCubeObjects = GameObject.FindGameObjectsWithTag("Orange");
        orangeCubesTransforms = new List<Transform>();

        foreach (GameObject orangeCubeObject in orangeCubeObjects)
        {
            orangeCubesTransforms.Add(orangeCubeObject.transform);
        }
    }

    //makes a list of positions from transforms
    public List<Vector3> OrangeCubePositions
    {
        get
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (Transform transform in orangeCubesTransforms)
            {
                positions.Add(transform.position);
            }
            return positions;
        }
    }


    //finds the closest orange cube
    private Transform FindClosestOrangeCube(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestOrangeCube = null;

        foreach (Transform orangeCube in orangeCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, orangeCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold)
            {
                closestDistance = distance;
                closestOrangeCube = orangeCube;
            }
        }

        return closestOrangeCube;
    }

    //finds the closest orange cube that is not underneath the AI agent
    private Transform FindClosestOrangeCubeNotUnderneath(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestOrangeCube = null;

        foreach (Transform orangeCube in orangeCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, orangeCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold && !IsMainAgentOnOrangeCube(mainAgentPosition, orangeCube.position))
            {
                closestDistance = distance;
                closestOrangeCube = orangeCube;
            }
        }

        return closestOrangeCube;
    }

    //gets the direction in which there is the orange cube
    private string GetDirectionToClosestOrangeCube(Vector3 mainAgentPosition, Vector3 orangeCubePosition)
    {
        Vector3 directionVector = orangeCubePosition - mainAgentPosition;
        float x = Mathf.Round(directionVector.x);
        float z = Mathf.Round(directionVector.z);

        if (Mathf.Abs(x) > Mathf.Abs(z))
        {
            return x > 0 ? "right" : "left";
        }
        else
        {
            return z > 0 ? "up" : "down";
        }
    }

    //verifies if the AI agent is on the orange cube
    private bool IsMainAgentOnOrangeCube(Vector3 mainAgentPosition, Vector3 orangeCubePosition)
    {
        float distance = Vector3.Distance(mainAgentPosition, orangeCubePosition);
        return distance <= 0.3f;
    }

    //function for responding to AI agent
    public void RespondToMainAgent()
    {
        Transform closestOrangeCube = FindClosestOrangeCube(mainAgentTransform.position); //either underneath or next to agent
        Transform closestOrangeCubeNotUnderneath = FindClosestOrangeCubeNotUnderneath(mainAgentTransform.position); // only next to agent

        if (closestOrangeCube != null)
        {
            if (IsMainAgentOnOrangeCube(mainAgentTransform.position, closestOrangeCube.position))
            {
                if (closestOrangeCubeNotUnderneath != null) // if there is an orange cube not underneath the agent
                {
                    string direction = GetDirectionToClosestOrangeCube(mainAgentTransform.position, closestOrangeCubeNotUnderneath.position);
                    DisplayMessage($"Go {direction}");
                }
                else // if there is no orange cube next to the agent
                {
                    DisplayMessage("No nearby blocks");
                }
            }
            else // if the agent is not on an orange cube
            {
                string direction = GetDirectionToClosestOrangeCube(mainAgentTransform.position, closestOrangeCube.position);
                DisplayMessage($"Go {direction}");
            }
        }
        else
        {
            DisplayMessage("No nearby blocks");
        }
    }

    //function that displays the message of the orange agent
    public void DisplayMessage(string message)
    {
        messageText.text = message;

        if (message == "")
        {
            orangeAgentTurret.material = normalMaterial;
        }
        else if (message != "")
        {
            orangeAgentTurret.material = messageMaterial;
        }
    }
}
