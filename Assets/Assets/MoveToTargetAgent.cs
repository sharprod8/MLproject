using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToTargetAgent : Agent
{
    [Header("Team")]
    [SerializeField] public TeamColour team;

    [Header("Zones")]
    [SerializeField] private ZoneScript homeZone;
    [SerializeField] private ZoneScript enemyZone;

    [Header("rend")]
    [SerializeField] private Renderer agentRenderer;

    [Header("speed")]
    [SerializeField] private float moveSpeed = 4f;

    [SerializeField] private BoxSpawner boxSpawner;
    private Box carriedBox;

    private void Start()
    {
        switch (team)
        {
            case TeamColour.Red:
                agentRenderer.material.color = Color.red;
                break;

            case TeamColour.Blue:
                agentRenderer.material.color = Color.blue;
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        carriedBox = null;

        transform.position = homeZone.transform.position + new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f));

        if (team == TeamColour.Red)
        {
            boxSpawner.ResetBoxes();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        Box nearestBox = GetNearestBox();

        if (nearestBox != null)
        {
            sensor.AddObservation(nearestBox.transform.localPosition);
            sensor.AddObservation((int)nearestBox.Owner);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0);
        }

        sensor.AddObservation(homeZone.transform.localPosition);
        sensor.AddObservation(enemyZone.transform.localPosition);
        sensor.AddObservation(carriedBox != null ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float grab = actions.ContinuousActions[2];

        transform.position += new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;

        AddReward(-0.0005f);

        AddReward(GetOwnedBoxCount() * 0.0002f);

        if (carriedBox == null)
        {
            TryPickup(grab);
        }

        if (carriedBox != null)
        {
            carriedBox.transform.position = transform.position + new Vector3(0f, 0.5f, 1f);
        }
    }

    private void TryPickup(float grabValue)
    {
        if (grabValue < 0.5f)
            return;

        Box nearestBox = GetNearestBox();

        if (nearestBox == null)
            return;

        if (nearestBox.IsCarried)
            return;

        float distance = Vector3.Distance(transform.position, nearestBox.transform.position);

        if (distance < 1f)
        {
            carriedBox = nearestBox;
            carriedBox.IsCarried = true;

            AddReward(0.2f);
        }
    }

    public void TryDeliverBox(ZoneScript zone)
    {
        if (carriedBox == null)
            return;

        //eweqew
        if (zone.Team != team)
            return;

        TeamColour previousOwner =
            carriedBox.Owner;

        carriedBox.SetOwner(team);

        if (previousOwner == TeamColour.None)
        {
            AddReward(1f);
        }
        else if (previousOwner != team)
        {
            AddReward(2f);
        }

        carriedBox.IsCarried = false;

        carriedBox.transform.position =
            zone.transform.position +
            new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.5f,
                Random.Range(-0.3f, 0.3f));

        carriedBox = null;
    }

    /*public void BoxDelivered()
    {
        if (carriedBox == null)
            return;

        TeamColour previousOwner = carriedBox.Owner;

        carriedBox.SetOwner(team);

        if (previousOwner != team)
        {
            AddReward(1f);
        }

        carriedBox.IsCarried = false;
        carriedBox = null;
    }*/

    private Box GetNearestBox()
    {
        Box[] boxes = FindObjectsByType<Box>(FindObjectsSortMode.None);
        Box closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Box box in boxes)
        {
            if (box.IsCarried)
                continue;

            float distance = Vector3.Distance(transform.position, box.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = box;
            }
        }

        return closest;
    }

    private int GetOwnedBoxCount()
    {
        Box[] boxes = FindObjectsByType<Box>(FindObjectsSortMode.None);

        int ownedBoxes = 0;

        foreach (Box box in boxes)
        {
            if (box.Owner == team)
            {
                ownedBoxes++;
            }
        }

        return ownedBoxes;
    }

    public override void Heuristic(
        in ActionBuffers actionsOut)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}