
using UnityEngine;

public class Box : MonoBehaviour
{
    [Header("Oowner")]
    [SerializeField] private TeamColour owner = TeamColour.None;

    [Header("e")]
    [SerializeField] private Renderer boxRenderer;

    private Rigidbody rb;

    
    private bool isCarried;

    public bool IsCarried
    {
        get { return isCarried; }

        set
        {
            isCarried = value;

            if (rb != null)
            {
                
                rb.isKinematic = value;

                if (value)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    public TeamColour Owner
    {
        get { return owner; }
    }

    private void Awake()
    {
        if (boxRenderer == null)
            boxRenderer = GetComponent<Renderer>();

        rb = GetComponent<Rigidbody>();

        UpdateColour();
    }

    public void SetOwner(TeamColour newOwner)
    {
        owner = newOwner;

        UpdateColour();
    }

    private void UpdateColour()
    {
        if (boxRenderer == null)
            return;

        switch (owner)
        {
            case TeamColour.Red:
                boxRenderer.material.color = Color.red;
                break;

            case TeamColour.Blue:
                boxRenderer.material.color = Color.blue;
                break;

            case TeamColour.None:
                boxRenderer.material.color = Color.white;
                break;
        }
    }
    public void ResetBox(Vector3 position, Quaternion rotation)
    {
        
        IsCarried = false;

   
        SetOwner(TeamColour.None);

        // .
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetPositionAndRotation(position, rotation);

        
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
        }
    }
}
