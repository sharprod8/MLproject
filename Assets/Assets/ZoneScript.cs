using UnityEngine;

public class ZoneScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Box box = other.GetComponent<Box>();

        if (box != null)
        {
            MoveToTargetAgent agent = FindAnyObjectByType<MoveToTargetAgent>();
            Debug.Log("box collided");

            if (agent != null)
            {
                agent.BoxDelivered();
                Debug.Log("delivered");
            }
        }
    }
}
