using UnityEngine;

public class ZoneScript : MonoBehaviour
{
    public TeamColour Team;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();

        if (Team == TeamColour.Red)
            rend.material.color = Color.red;

        if (Team == TeamColour.Blue)
            rend.material.color = Color.blue;
    }

    private void OnTriggerEnter(Collider other)
    {
        MoveToTargetAgent agent =
            other.GetComponent<MoveToTargetAgent>();

        if (agent == null)
            return;

        agent.TryDeliverBox(this);
    }
}