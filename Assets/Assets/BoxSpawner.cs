using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [SerializeField] private Box[] boxes;

    public void ResetBoxes()
    {
        foreach (Box box in boxes)
        {
            box.IsCarried = false;

            box.SetOwner(TeamColour.None);

            Vector3 randomOffset = new Vector3(Random.Range(-7f, 7f), 0.5f, Random.Range(-7f, 7f));

            box.transform.position = transform.position + randomOffset;
        }
    }
}