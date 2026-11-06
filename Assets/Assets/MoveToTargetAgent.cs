using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToTargetAgent : Agent
{
    [Header("Team")]
    [SerializeField] public TeamColour team;
    [SerializeField] private bool boxResetter;

    [Header("Zones")]
    [SerializeField] private ZoneScript homeZone;
    [SerializeField] private ZoneScript enemyZone;

    [Header("rend")]
    [SerializeField] private Renderer agentRenderer;

    [Header("speed")]
    [SerializeField] private float moveSpeed = 4f;

    [SerializeField] private BoxSpawner boxSpawner;
    private Box carriedBox;
    private int successfulPickups;
    private int successfulDeliveries;
    private float rewardTimer;
    

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

        if (boxResetter == true)
        {
            boxSpawner.ResetBoxes();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        
        if (carriedBox == null)
        {
            Box nearestBox = GetNearestBox();

            if (nearestBox != null)
            {
                sensor.AddObservation(nearestBox.transform.localPosition - transform.localPosition);
                sensor.AddObservation((int)nearestBox.Owner);
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(0);
            }
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0);
        }


        Vector3 toHome = homeZone.transform.localPosition - transform.localPosition;
        Vector3 toEnemy = enemyZone.transform.localPosition - transform.localPosition;

        sensor.AddObservation(toHome);
        sensor.AddObservation(toEnemy);
        sensor.AddObservation(carriedBox != null ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        //float grab = actions.ContinuousActions[2];

        //rewardTimer += Time.deltaTime;

        /*if (rewardTimer > 1f)
        {
            rewardTimer = 0f;
            AddReward(GetOwnedBoxCount() * 0.05f);
        }
        */
        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        transform.position += movement * moveSpeed * Time.deltaTime;

        AddReward(-0.0005f);

        //AddReward(GetOwnedBoxCount() * 0.001f);

        if (carriedBox == null)
        {
            TryPickup();
        }

        if (carriedBox != null)
        {
            carriedBox.transform.position = transform.position + new Vector3(0f, 0.8f, 0f);
        }
    }

    private void TryPickup()
    {
        //if (grabValue < 0.5f) return;

        if (carriedBox != null) return;

        Box nearestBox = GetNearestBox();

        if (nearestBox == null) return;

        if (nearestBox.IsCarried) return;

        float distance = Vector3.Distance(transform.position, nearestBox.transform.position);

        if (distance < 1.25f)
        {
            carriedBox = nearestBox;
            carriedBox.IsCarried = true;

            successfulPickups++;

            if (nearestBox.Owner != team)
                AddReward(0.1f);

            Debug.Log("Carried");
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
            AddReward(2f);
        }
        else if (previousOwner != team)
        {
            AddReward(3f);
        }
        else
        {
            //nothing
        }

        carriedBox.IsCarried = false;

        carriedBox.transform.position =
            zone.transform.position +
            new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.5f,
                Random.Range(-0.3f, 0.3f));

        carriedBox = null;

        Debug.Log("Delivered");
    }

    private Box GetNearestBox()
    {
        Box[] boxes = FindObjectsByType<Box>(FindObjectsSortMode.None);
        Box closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Box box in boxes)
        {
            if (box.IsCarried)
                continue;

            if (box.Owner == team) continue;

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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = 0;
        actions[1] = 0;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    private void OnDrawGizmos()
    {
        Box nearest = GetNearestBox();

        if (nearest != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nearest.transform.position);
        }
    }
}

//mlagents-learn config/MoveToTarget.yaml --run-id=Home_02 --force