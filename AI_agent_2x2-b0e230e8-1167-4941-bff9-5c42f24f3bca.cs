using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;


public class AI_agent_2x2 : Agent
{


    //the transform of the agent to move
    public Transform targetTransform;

    public Material win_Material;
    public Material lose_Material;
    public Material normal_Material;

    public MeshRenderer floor_MeshRenderer;

    private float angle;

    private int lastRotation;

    private bool rotationCooldown = false;
    private bool movementCooldown = false;
    private bool dialogueCooldown = false;

    public TextMeshProUGUI mainAgentText;

    public Red_agent redAgent;
    public Green_agent greenAgent;

    private bool performedAction;

    private int rotationCount;

    private int dialogue_cnt;


    //called when a new episode begins
    public override void OnEpisodeBegin()
    {
        
        transform.localPosition = new Vector3(-1.20224f, 0.3026757f, 1.125372f);

        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        floor_MeshRenderer.material = normal_Material;

        angle = 180;

        lastRotation = -1;

        performedAction = true;

        ResetUsedTiles("Used Green", "Green");
        ResetUsedTiles("Used Red", "Red");

        rotationCount = 0;

        dialogue_cnt = 0;

    }
    //resets used tiles
    private void ResetUsedTiles(string usedTag, string originalTag)
    {
        GameObject[] usedTiles = GameObject.FindGameObjectsWithTag(usedTag);
        foreach (GameObject tile in usedTiles)
        {
            tile.tag = originalTag;
        }
    }

    //colects the observations
    public override void CollectObservations(VectorSensor sensor)
    {
        //underneath is the sensor for the AI to know its position
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);

        //underneath is where the goal position info is
        sensor.AddObservation(targetTransform.position);


        //add red agent message as an observation
        int redAgentMessage = GetMessageNumber(redAgent.messageText.text);
        sensor.AddObservation(redAgentMessage);


        //add green agent message as an observation
        int greenAgentMessage = GetMessageNumber(greenAgent.messageText.text);
        sensor.AddObservation(greenAgentMessage);
    }

    //changes message of agent into integer
    private int GetMessageNumber(string message)
    {
        switch (message)
        {
            case "No nearby blocks":
                return 1;
            case "Go up":
                return 2;
            case "Go down":
                return 3;
            case "Go left":
                return 4;
            case "Go right":
                return 5;
            default:
                return 0;
        }
    }


    //called when the agent receives an action by the neural network
    public override void OnActionReceived(ActionBuffers actions)
    {
        //get the action for movement direction
        int rotation = actions.DiscreteActions[0];

        //get the action for moving 
        int moving = actions.DiscreteActions[1];

        //get the action for dialogue
        int dialogue = actions.DiscreteActions[2];

        //maximum rotations on a specific tile
        if (rotationCount >= 10)
        {
            floor_MeshRenderer.material = lose_Material;
            AddReward(-5.0f);

            StartCoroutine(EndEpisodeDelayed(1.0f));
        }


        if (!rotationCooldown)
        {
            //check if the agent has not reached the maximum number of rotations allowed
            //Debug.Log(rotationCount);

            //move the agent based on the selected direction
            switch (rotation)
            {
                case 0:
                    //no rotation selected
                    break;

                case 1:
                    //rotate to the left
                    transform.Rotate(0f, -90f, 0f);
                    angle -= 90f;
                    AddReward(-0.3f);

                    if (lastRotation == 1)
                    {
                        AddReward(-0.4f); //additional penalty for consecutive left rotations
                    }

                    lastRotation = 1;

                    rotationCount++;
                    break;

                case 2:
                    //rotate to the right
                    transform.Rotate(0f, +90f, 0f);
                    angle += 90f;
                    AddReward(-0.3f);

                    if (lastRotation == 2)
                    {
                        AddReward(-0.4f); //additional penalty for consecutive right rotations
                    }

                    lastRotation = 2;

                    rotationCount++;
                    break;
            }

            if (rotation != 0) //only start cooldown if a rotation action was executed
            {
                StartCoroutine(RotationCooldown());
                performedAction = true;
            }
        }



        if (!movementCooldown)
        {

            switch (moving)
            {

                case 0:
                    //no movement selected
                    break;

                case 1:
                    //move forward

                    Vector3 initialPosition = transform.localPosition;


                    if (angle==0)
                    {
                        transform.localPosition += new Vector3(0f, 0f, 2.496372f);
                        AddReward(-0.3f);
                    }
                    else if (angle==90)
                    {
                        transform.localPosition += new Vector3(2.40448f, 0f, 0f) ;
                        AddReward(-0.3f);
                    }
                    else if (angle == 180)
                    {
                        transform.localPosition += new Vector3(0f, 0f, -2.496372f) ;
                        AddReward(-0.3f);
                    }
                    else if (angle == 270)
                    {
                        transform.localPosition += new Vector3(-2.40448f, 0f, 0f);
                        AddReward(-0.3f);

                    }


                    if (initialPosition != transform.localPosition)
                    {
                        //Debug.Log("Forward");
                        rotationCount = 0;
                    }

                    break;

            }

            if (moving != 0) //only start cooldown if a movement action was executed
            {
                StartCoroutine(MovementCooldown());
                performedAction = true;
            }
        }

        if (!dialogueCooldown && dialogue_cnt==0)
        {
            if (performedAction == true)
            {
                switch (dialogue)
                {
                    case 0:
                        //do not display a message
                        mainAgentText.text = "";
                        break;

                    case 1:
                        //display a message about the red agent

                        mainAgentText.text = "Where should I go agent 2?";
                        redAgent.RespondToMainAgent();

                        //redAgentMessageScript.DisplayMessage("Message about Red Agent");

                        break;

                    case 2:
                        //display a message about the green agent

                        mainAgentText.text = "Where should I go agent 1?";
                        greenAgent.RespondToMainAgent();

                        //greenAgentMessageScript.DisplayMessage("Message about Green Agent");

                        break;
                }
            }

            if (dialogue != 0 && performedAction == true) //only start cooldown if a dialogue action was executed
            {

                AddReward(0.1f); 
                
                performedAction = false; //reset after the dialogue action

                StartCoroutine(DialogueCooldown());
            }
        }


    }

    //delays the rotation for easy vizualization
    private IEnumerator RotationCooldown()
    {
        rotationCooldown = true;

        //wait for the cooldown duration
        yield return new WaitForSeconds(0.9f); 

        rotationCooldown = false;
    }
    //delays the movement for easy vizualization
    private IEnumerator MovementCooldown()
    {
        movementCooldown = true;

        //wait for the cooldown duration
        yield return new WaitForSeconds(0.9f); 

        movementCooldown = false;
    }
    //delays the dialogue for easy vizualization
    private IEnumerator DialogueCooldown()
    {
        dialogueCooldown = true;

        //wait for the cooldown duration
        yield return new WaitForSeconds(1.3f); 

        dialogueCooldown = false;

        //clear the messages after the cooldown
        mainAgentText.text = "";
        redAgent.DisplayMessage("");
        greenAgent.DisplayMessage("");
    }
    //lets you to play with the AI agent by pressing keyboard keys
    public override void Heuristic(in ActionBuffers actionsOut)
    {


        var discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 0;
        discreteActions[1] = 0;
        discreteActions[2] = 0;

        //if you hold the key for rotations, the agent will stop moving forward
        if (Input.GetKey(KeyCode.LeftArrow))//go left
        {
            discreteActions[0] = 1;
            

        }
        else if (Input.GetKey(KeyCode.RightArrow))//go right
        {
            discreteActions[0] = 2;
            

        }
        else if (Input.GetKey(KeyCode.UpArrow))//go up
        {
            discreteActions[1] = 1;


        }
        else if (Input.GetKey(KeyCode.R))//speak to red agent
        {
            discreteActions[2] = 1;


        }
        else if (Input.GetKey(KeyCode.G))//speak to green agent
        {
            discreteActions[2] = 2;


        }




    }


    //when the AI agent collides with:
    private void OnTriggerEnter(Collider other)
    {
        //the end tile
        if (other.CompareTag("Cube"))
        {
            //Debug.Log("Collision detected with cube!");
            floor_MeshRenderer.material = win_Material;
            AddReward(+5.0f);

            StartCoroutine(EndEpisodeDelayed(1.0f));
            
        }
        //a barrier
        if (other.CompareTag("Barrier"))
        {
            //Debug.Log("Barrier");
            floor_MeshRenderer.material = lose_Material;
            AddReward(-5.0f);

            StartCoroutine(EndEpisodeDelayed(1.0f));


        }
        //a green tile
        if (other.CompareTag("Green"))
        {
            //Debug.Log("Green");
            
            AddReward(+2.0f);
            other.tag = "Used Green";



        }
        //a red tile
        if (other.CompareTag("Red"))
        {
            //Debug.Log("Red");
            
            AddReward(-2.0f);
            other.tag = "Used Red";



        }

    }
    //delays the end of episode for easy vizualization
    private IEnumerator EndEpisodeDelayed(float delay)
    {
        dialogue_cnt = 1;
        yield return new WaitForSeconds(delay);
        EndEpisode();
    }

}
