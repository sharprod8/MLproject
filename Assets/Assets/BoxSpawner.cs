
using UnityEngine;
using System.Collections.Generic;

public class BoxSpawner : MonoBehaviour
{
    [Header("BOXES")]
    [SerializeField] private Box[] spawnedBoxes;

    // NEW: Store each box's original position.
    private Dictionary<Box, Vector3> originalPositions =
        new Dictionary<Box, Vector3>();

    // NEW: Store each box's original rotation.
    private Dictionary<Box, Quaternion> originalRotations =
        new Dictionary<Box, Quaternion>();

    private void Awake()
    {

        if (spawnedBoxes == null || spawnedBoxes.Length == 0)
        {
            spawnedBoxes = FindObjectsByType<Box>(
                FindObjectsSortMode.None
            );
        }

       
        foreach (Box box in spawnedBoxes)
        {
            if (box == null)
                continue;

            originalPositions[box] = box.transform.position;
            originalRotations[box] = box.transform.rotation;
        }
    }

    
    public void ResetBoxes()
    {
        foreach (Box box in spawnedBoxes)
        {
            if (box == null)
                continue;

            if (!originalPositions.ContainsKey(box))
                continue;

            
            box.ResetBox(
                originalPositions[box],
                originalRotations[box]
            );
        }

        Physics.SyncTransforms();

        Debug.Log("resetted");
    }
}
