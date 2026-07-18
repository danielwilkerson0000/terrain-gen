using UnityEngine;

public class Face : MonoBehaviour
{
    public static int idCount = 1;

    public int id;
    public Vector3 normal;
    public Vector3 Pos => transform.position;
    public Tile tile;

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

    public void SetTile(Tile tile)
    {
        this.tile = tile;
    }

    public override string ToString()
    {
        return $"Face[{id}]";
    }

    public void PutTile(Tile tile)
    {
        SetTile(tile);
        tile.PutOn(this);
    }
}