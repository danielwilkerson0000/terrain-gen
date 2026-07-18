using UnityEngine;

public class Tile : MonoBehaviour
{
    GameObject tile;
    public static float scale = 0.3f;
    public static int idCount = 1;
    public Face face;
    public int id;

    public Color color;

    public Tile()
    {
        SetID();
        color = new(0.8f, 0.8f, 0.8f);
    }

    public void Awake()
    {
        InitWidgets();
    }

    public void InitWidgets()
    {
        tile ??= GameObject.CreatePrimitive(PrimitiveType.Cube);

        tile.transform.SetParent(transform);
        tile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        tile.GetComponent<Renderer>().sharedMaterial.color = color;
        tile.transform.localScale = scale * Vector3.one;
    }

    public void Update()
    {
    }

    public void SetID()
    {
        id = idCount;
        idCount++;
    }

    public void Color(Color c)
    {
        color = c;

        if (tile == null) return;
        tile.GetComponent<Renderer>().material.color = color;
    }

    public void PutOn(Face face)
    {
        this.face = face;

        if (tile == null) return;

        tile.transform.SetParent(face.transform);
        tile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public override string ToString()
    {
        return $"Tile[{id}]";
    }
}