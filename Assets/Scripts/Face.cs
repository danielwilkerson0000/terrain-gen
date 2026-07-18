using UnityEngine;

public class Face : MonoBehaviour
{
    public static int idCount = 1;

    public int id;
    public Vector3 Pos => transform.position;

    public Face()
    {
        SetID();
    }

    public void SetID()
    {
        id = idCount;
        idCount++;
    }

    public override string ToString()
    {
        return $"Face[{id}]";
    }
}