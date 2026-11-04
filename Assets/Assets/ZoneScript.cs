using UnityEngine;

public class ZoneScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Box box = other.GetComponent<Box>();

        if (box != null)
        {
            MoveToTargetAgent agent = FindObjectOfType<MoveToTargetAgent>();

            if (agent != null)
            {
                agent.BoxDelivered();
            }
        }
    }
}
