using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class Red_agent : Agent
{
    //all red cubes data
    public List<Transform> redCubesTransforms;

    //message data
    public TextMeshProUGUI messageText;

   
    public Renderer redAgentTurret;

    //materials used for better visualization
    public Material normalMaterial;
    public Material messageMaterial;

    //distance to a nearby cube
    public float distanceThreshold = 2.6f;
    public float maxDistanceThreshold = 1.2f;


    //AI agent data
    public Transform mainAgentTransform;



    //finds the red cubes in the environment
    private void Awake()
    {
        GameObject[] redCubes = GameObject.FindGameObjectsWithTag("Red");
        redCubesTransforms = new List<Transform>();

        foreach (GameObject redCube in redCubes)
        {
            redCubesTransforms.Add(redCube.transform);
        }
    }

    //makes a list of positions of all the red cubes
    public List<Vector3> RedCubePositions
    {
        get
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (Transform transform in redCubesTransforms)
            {
                positions.Add(transform.position);
            }
            return positions;
        }
    }


    //finds the closest red cube
    private Transform FindClosestRedCube(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestRedCube = null;

        foreach (Transform redCube in redCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, redCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold)
            {
                closestDistance = distance;
                closestRedCube = redCube;
            }
        }

        return closestRedCube;
    }

    //finds the closest red cube that is not underneath the AI agent
    private Transform FindClosestRedCubeNotUnderneath(Vector3 mainAgentPosition)
    {
        float closestDistance = distanceThreshold;
        Transform closestRedCube = null;

        foreach (Transform redCube in redCubesTransforms)
        {
            float distance = Vector3.Distance(mainAgentPosition, redCube.position);
            if (distance < closestDistance && distance <= maxDistanceThreshold && !IsMainAgentOnRedCube(mainAgentPosition, redCube.position))
            {
                closestDistance = distance;
                closestRedCube = redCube;
            }
        }

        return closestRedCube;
    }

    //gets the direction in which there is the red cube
    private string GetDirectionToClosestRedCube(Vector3 mainAgentPosition, Vector3 redCubePosition)
    {
        Vector3 directionVector = redCubePosition - mainAgentPosition;
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

    //verifies if the AI agent is on the red cube
    private bool IsMainAgentOnRedCube(Vector3 mainAgentPosition, Vector3 redCubePosition)
    {
        float distance = Vector3.Distance(mainAgentPosition, redCubePosition);
        return distance <= 0.3f;
    }



    //private void LogDistanceBetweenRedCubeAndMainAgent(Vector3 mainAgentPosition, Vector3 redCubePosition)
    //{
    //    float distance = Vector3.Distance(mainAgentPosition, redCubePosition);
    //    Debug.Log($"Distance between Main Agent and Red Cube: {distance}");
    //}



    //function for responding to AI agent
    public void RespondToMainAgent()
    {
        Transform closestRedCube = FindClosestRedCube(mainAgentTransform.position); //either underneath or next to agent
        Transform closestRedCubeNotUnderneath = FindClosestRedCubeNotUnderneath(mainAgentTransform.position); // only next to agent

        if (closestRedCube != null)
        {

            //LogDistanceBetweenRedCubeAndMainAgent(mainAgentTransform.position, closestRedCube.position);

            if (IsMainAgentOnRedCube(mainAgentTransform.position, closestRedCube.position))
            {
                if (closestRedCubeNotUnderneath != null) // if there is a red cube not underneath the agent
                {
                    string direction = GetDirectionToClosestRedCube(mainAgentTransform.position, closestRedCubeNotUnderneath.position);
                    DisplayMessage($"Go {direction}");
                }
                else // if there is no red cube next to the agent
                {
                    DisplayMessage("No nearby blocks");
                }
            }
            else // if the agent is not on a red cube
            {
                string direction = GetDirectionToClosestRedCube(mainAgentTransform.position, closestRedCube.position);
                DisplayMessage($"Go {direction}");
            }
        }
        else
        {
            DisplayMessage("No nearby blocks");
        }

    }

    //function that displays the message of the red agent
    public void DisplayMessage(string message)
    {
        messageText.text = message;

        if (message=="")
        {
            redAgentTurret.material = normalMaterial;
        }
        else if(message!="")
        {
            redAgentTurret.material = messageMaterial;
        }
    }

}
