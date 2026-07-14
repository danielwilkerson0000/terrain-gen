using UnityEngine;

public class Face : MonoBehaviour
{
    public static int idCount = 0;

    public int id;
    public Vector3 normal;
    public Vector3 Pos => transform.position;
    public Tile occupier;

    public Face()
    {
        SetID();
    }

    public void SetID()
    {
        id = idCount;
        idCount++;
    }

    private void Awake()
    {
        normal = transform.forward;
    }

    public void SetOccupiedTile(Tile tile) {
        occupier = tile;
    }
}