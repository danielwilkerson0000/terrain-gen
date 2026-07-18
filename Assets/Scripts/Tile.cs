using UnityEngine;

public class Tile : MonoBehaviour
{
    public static int idCount = 1;
    public Face face;
    public Renderer Renderer_ => GetComponent<Renderer>();
    public int id;

    public Color color;

    public Tile()
    {
        SetID();
        color = new(0.8f, 0.8f, 0.8f);
    }



    public void SetID()
    {
        id = idCount;
        idCount++;
    }

    public void Color(Color c)
    {
        color = c;

        Renderer_.material.color = color;
    }

    public void PutOn(Face face)
    {
        this.face = face;

        transform.SetParent(face.transform, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public static Tile Basic(float scale) {
        GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Tile tile = tileObject.AddComponent<Tile>();
        tileObject.name = $"Tile[{tile.id}]";
        tileObject.transform.localScale = scale * Vector3.one;

        return tile;
    }

    public override string ToString()
    {
        return $"Tile[{id}]";
    }
}