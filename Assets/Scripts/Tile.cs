using UnityEngine;

class Tile
{
    public Color color;
    public Tile()
    {
        color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); 
    }
}