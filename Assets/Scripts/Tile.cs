using UnityEngine;

public class Tile : MonoBehaviour
{
    public Face ownerFace;
    public int id;

    public Color color;
    public Tile()
    {
        color = new(0.8f, 0.8f, 0.8f);
        if (ownerFace != null)
        {
            ownerFace.occupier = this;
        }
    }

    public void Color(Color c)
    {
        color = c;
    }


    public void SetOwner(Face newOwner)
    {
        ownerFace = newOwner;
        newOwner.SetOccupiedTile(this);
    }
}