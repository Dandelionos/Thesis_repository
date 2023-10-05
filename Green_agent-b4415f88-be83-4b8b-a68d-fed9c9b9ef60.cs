using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class Green_agent : Agent
{
    //all green cubes data
    public List<Transform> greenCubesTransforms;

    //message data
    public TextMeshProUGUI messageText;

    public Renderer greenAgentTurret;

    //materials used for better visualization
    public Material normalMaterial;
    public Material messageMaterial;

    //distance to a nearby cube
    public float distanceThreshold = 2.6f;
    public float maxDistanceThreshold = 1.2f;


    //AI agent data
    public Transform mainAgentTransform;

    //finds the green cubes in the environment
    private void Awake()
    {
        GameObject[] greenCubeObjects = GameObject.FindGameObjectsWithTag("Green");
        greenCubesTransforms = new List<Transform>();

        foreach (GameObject greenCubeObject in greenCubeObjects)
        {
            greenCubesTransforms.Add(greenCubeObject.transform);
        }
    }

    //makes a list of positions of all the green cubes
    public List<Vector3> GreenCubePositions
    {
        get
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (Transform transform in greenCubesTransforms)
            {
                positions.Add(transform.position);
            }
            return positions;
        }
    }


    //finds the closest green cube
    private Transform FindClosestGreenCube(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestGreenCube = null;

        foreach (Transform greenCube in greenCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, greenCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold)
            {
                closestDistance = distance;
                closestGreenCube = greenCube;
            }
        }

        return closestGreenCube;
    }
    //finds the closest green cube that is not underneath the AI agent
    private Transform FindClosestGreenCubeNotUnderneath(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestGreenCube = null;

        foreach (Transform greenCube in greenCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, greenCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold && !IsMainAgentOnGreenCube(mainAgentPosition, greenCube.position))
            {
                closestDistance = distance;
                closestGreenCube = greenCube;
            }
        }

        return closestGreenCube;
    }

    //gets the direction in which there is the green cube
    private string GetDirectionToClosestGreenCube(Vector3 mainAgentPosition, Vector3 greenCubePosition)
    {
        Vector3 directionVector = greenCubePosition - mainAgentPosition;
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

    //verifies if the AI agent is on the green cube
    private bool IsMainAgentOnGreenCube(Vector3 mainAgentPosition, Vector3 greenCubePosition)
    {
        float distance = Vector3.Distance(mainAgentPosition, greenCubePosition);
        return distance <= 0.3f;
    }

    //function for responding to AI agent
    public void RespondToMainAgent()
    {
        Transform closestGreenCube = FindClosestGreenCube(mainAgentTransform.position); //either underneath or next to agent
        Transform closestGreenCubeNotUnderneath = FindClosestGreenCubeNotUnderneath(mainAgentTransform.position); // only next to agent

        if (closestGreenCube != null)
        {
            if (IsMainAgentOnGreenCube(mainAgentTransform.position, closestGreenCube.position))
            {

                if (closestGreenCubeNotUnderneath != null) // if there is a green cube not underneath the agent
                {
                    string direction = GetDirectionToClosestGreenCube(mainAgentTransform.position, closestGreenCubeNotUnderneath.position);
                    DisplayMessage($"Go {direction}");
                }
                else // if there is no green cube next to the agent
                {
                    DisplayMessage("No nearby blocks");
                }
            }
            else // if the agent is not on a green cube
            {
                string direction = GetDirectionToClosestGreenCube(mainAgentTransform.position, closestGreenCube.position);
                DisplayMessage($"Go {direction}");
            }
        }
        else
        {
            DisplayMessage("No nearby blocks");
        }
    }

    //function that displays the message of the green agent
    public void DisplayMessage(string message)
    {
        messageText.text = message;

        if (message == "")
        {
            greenAgentTurret.material = normalMaterial;
        }
        else if (message != "")
        {
            greenAgentTurret.material = messageMaterial;
        }
    }

}
