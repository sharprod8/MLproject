using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MoveToTargetAgent : Agent
{

    [Header("Team")]
    [SerializeField] public TeamColour team;
    //erializeField] private bool boxResetter;

    [Header("Zones")]
    [SerializeField] private ZoneScript homeZone;
    [SerializeField] private ZoneScript enemyZone;

    [Header("rend")]
    [SerializeField] private Renderer agentRenderer;

    [Header("speed")]
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;
    private Vector3 movementInput;

    //[SerializeField] private BoxSpawner boxSpawner;
    private Box carriedBox;
    private int successfulPickups;
    private int successfulDeliveries;
    private float rewardTimer;

    bool hitWallInEpisode;
    

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

private void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector3 movement =
            movementInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    private void LateUpdate()
    {
        if (carriedBox != null)
        {
            carriedBox.transform.position =
                transform.position +
                new Vector3(0f, 0.8f, 0f);
        }
    }


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (team == TeamColour.Red)
        {
            agentRenderer.material.color = Color.red;
        }
        else if (team == TeamColour.Blue)
        {
            agentRenderer.material.color = Color.blue;
        }

if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAgent(this);
        }
    }

    public override void OnEpisodeBegin()
    {
if (carriedBox != null)
        {
            carriedBox.IsCarried = false;
            carriedBox = null;
        }

        movementInput = Vector3.zero;

wallTriggeredThisEpisode = false;

transform.position =
            homeZone.transform.position +
            new Vector3(
                Random.Range(-1f, 1f),
                0.5f,
                Random.Range(-1f, 1f)
            );

if (rb != null)
        {
            rb.position = transform.position;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

Physics.SyncTransforms()

        //if (boxResetter == true)
            //boxSpawner.ResetBoxes();

        successfulDeliveries = 0;
        successfulPickups = 0;
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

       
        
        movementInput =
            new Vector3(moveX, 0f, moveZ).normalized;

        AddReward(-0.0005f);

        if (carriedBox == null)
        {
            TryPickup();
        }
    }

    private void HandleWallContact()
    {
        if (wallTriggeredThisEpisode)
            return;

        wallTriggeredThisEpisode = true;

        AddReward(-0.5f);

        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndMatch("WALL COLLISION");
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

        if (zone.Team != team)
            return;

        TeamColour previousOwner = carriedBox.Owner;

        carriedBox.SetOwner(team);

        if (previousOwner == TeamColour.None)
        {
            AddReward(2f);
        }
        else if (previousOwner != team)
        {
            AddReward(3f);
        }

        carriedBox.IsCarried = false;

        carriedBox.transform.position =
            zone.transform.position +
            new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.5f,
                Random.Range(-0.3f, 0.3f)
            );

        carriedBox = null;

        successfulDeliveries++;

        Debug.Log(team + " delivered a box");
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
            AddReward(-1f);

            if (GameManager.instance != null)
            {
                GameManager.instance.EndMatch("WALL");
            }
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

    void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.UnregisterAgent(this);
        }
    }
}

//mlagents-learn config/MoveToTarget.yaml --run-id=Home_02 --force