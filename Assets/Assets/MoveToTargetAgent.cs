using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToTargetAgent : Agent
{
    //[SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Renderer floor;
    [SerializeField] private Color winColour = Color.green;
    [SerializeField] private Color loseColour = Color.red;
    [SerializeField] private Color idleColour = Color.grey;
    [SerializeField] private Box box;
    [SerializeField] private Transform zone;
    private Vector3 boxStartPosition;

    private bool carryingBox;

    private void Start()
    {
        boxStartPosition = box.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        carryingBox = false;
        box.IsCarried = false;

        transform.localPosition = new Vector3(
            Random.Range(-4f, 4f),
            0.5f,
            Random.Range(-4f, 4f));

        box.transform.localPosition = new Vector3(
            Random.Range(-4f, 4f),
            0.5f,
            Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(box.transform.localPosition);
        sensor.AddObservation(zone.localPosition);

        if (carryingBox)
        {
            sensor.AddObservation((float)1f);
        }
        else
        {
            sensor.AddObservation((float)0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float pickupAction = actions.ContinuousActions[2];
        Vector3 move = new Vector3(moveX, 0f, moveZ);
        transform.localPosition += move * moveSpeed * Time.deltaTime;
        // Small penalty every step so it learns to hurry
        if (MaxStep > 0) AddReward(-1f / MaxStep);

        if (!carryingBox)
        {
            float distanceToBox =
                Vector3.Distance(transform.position, box.transform.position);

            if (distanceToBox < 1f && pickupAction > 0.5f)
            {
                carryingBox = true;
                box.IsCarried = true;

                AddReward(0.1f);
            }
        }
    }

    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        var kb = Keyboard.current;

        if (kb == null) 
            return;

        if (kb.dKey.isPressed)
        {
            if (kb.aKey.isPressed)
            {
                continuous[0] = ((float)1f) - ((float)1f);
            }
            else
            {
                continuous[0] = ((float)1f) - ((float)0f);
            }
        }
        else
        {
            if (kb.aKey.isPressed)
            {
                continuous[0] = ((float)0f) - ((float)1f);
            }
            else
            {
                continuous[0] = ((float)0f) - ((float)0f);
            }
        }
        if (kb.wKey.isPressed)
        {
            if (kb.sKey.isPressed)
            {
                continuous[1] = ((float)1f) - ((float)1f);
            }
            else
            {
                continuous[1] = ((float)1f) - ((float)0f);
            }
        }
        else
        {
            if (kb.sKey.isPressed)
            {
                continuous[1] = ((float)0f) - ((float)1f);
            }
            else
            {
                continuous[1] = ((float)0f) - ((float)0f);
            }
        }
    }*/

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = 0;
        actions[1] = 0;
        actions[2] = 0;

        var kb = Keyboard.current;

        if (kb == null)
            return;

        if (kb.aKey.isPressed)
            actions[0] = -1;

        if (kb.dKey.isPressed)
            actions[0] = 1;

        if (kb.sKey.isPressed)
            actions[1] = -1;

        if (kb.wKey.isPressed)
            actions[1] = 1;

        if (kb.spaceKey.isPressed)
            actions[2] = 1;
    }

    public override void Initialize() // runs once at startup
    {
        floor.material.color = idleColour;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("Target"))
        {
            SetReward(1f);
            floor.material.color = winColour;
            EndEpisode();
        }*/

        if (other.CompareTag("Wall"))
        {
            SetReward(-1f);
            floor.material.color = loseColour;
            EndEpisode();
        }
    }

    private void Update()
    {
        if (carryingBox)
        {
            box.transform.position = transform.position + new Vector3(0f, 0.5f, 1f);
        }
    }

    public void BoxDelivered()
    {
        if (!carryingBox)
            return;

        AddReward(1f);
        floor.material.color = winColour;
        EndEpisode();
    }
}