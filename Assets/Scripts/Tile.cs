using Unity.VisualScripting;
using UnityEngine;

class Tile
{
    public static Tile Empty = new();

    public Color color;
    public Tile()
    {
        color = new(0.8f, 0.8f, 0.8f);
    }

    public void Color(Color c)
    {
        color = c;
    }
}