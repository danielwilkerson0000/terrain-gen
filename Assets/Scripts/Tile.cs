using UnityEngine;

public class Tile : MonoBehaviour
{
    public static int idCount = 1;
    public Face face;
    public int id;

    public Color color;
    public Tile()
    {
        SetID();
        color = new(0.8f, 0.8f, 0.8f);
        if (face != null)
        {
            face.tile = this;
        }
    }

    public void SetID()
    {
        id = idCount;
        idCount++;
    }

    public void Color(Color c)
    {
        color = c;
    }

    public void Place(Face face)
    {
        this.face = face;
        transform.SetParent(face.transform);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public override string ToString()
    {
        return $"Tile[{id}]";
    }
}