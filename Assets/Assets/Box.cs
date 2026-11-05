using UnityEngine;

public class Box : MonoBehaviour
{
    public bool IsCarried;
    public TeamColour Owner = TeamColour.None;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        UpdateColour();
    }

    public void SetOwner(TeamColour owner)
    {
        Owner = owner;
        UpdateColour();
    }

    private void UpdateColour()
    {
        switch (Owner)
        {
            case TeamColour.None:
                rend.material.color = Color.white;
                break;

            case TeamColour.Red:
                rend.material.color = Color.red;
                break;

            case TeamColour.Blue:
                rend.material.color = Color.blue;
                break;
        }
    }
}