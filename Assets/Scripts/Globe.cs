using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Globe : MonoBehaviour
{
    [HideInInspector]
    List<Face> faces;
    Dictionary<int, Tile> tiles;
    Globehedron globehedron;
    GlobeController controller;

    void Awake()
    {
        InitController();
        InitTiles();
    }

    void InitController()
    {
        controller = GetComponent<GlobeController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<GlobeController>();
        }

        controller.Initialize(this);
    }

    /// <summary>
    /// Naive search for the closest face to target point.
    /// </summary>
    public Face GetClosestFace(Vector3 target)
    {
        float dist = float.MaxValue;
        Face closest = null;
        foreach (Face face in faces)
        {
            Vector3 p = face.Pos;
            float m = (p - target).sqrMagnitude;
            if (m < dist)
            {
                dist = m;
                closest = face;
            }
        }

        return closest;
    }

    public float GetFaceScale()
    {
        if (globehedron == null) MakeGoldbergFaces(false);
        return globehedron.scale;
    }

    void InitTiles()
    {
        // creates + places faces in the world
        MakeGoldbergFaces(true);

        MakeTiles();
        ColorSomeTiles(0.2f);
    }

    public void MakeTiles()
    {
        tiles = new();
        foreach (Face face in faces)
        {
            Tile tile = face.gameObject.AddComponent<Tile>();
            tiles.Add(face.id, tile);

            face.PutTile(tile);
            tile.InitWidgets();
        }
    }

    public void ColorSomeTiles(float chance)
    {
        foreach (Tile tile in tiles.Values)
        {
            // give a few some color!
            if (Random.value < chance)
            {
                Color c = Random.ColorHSV(0, 1, 0.3f, 0.7f, 0.4f, 0.7f);
                tile.Color(c);
            }
        }
    }

    public void MakeGoldbergFaces(bool buildMeshes)
    {
        globehedron = transform.GetComponent<Globehedron>();
        globehedron.Generate(buildMeshes);
        faces = globehedron.faces;
    }

    public void SwapTiles(Face face1, Face face2)
    {
        Tile s = tiles[face1.id];
        Tile t = tiles[face2.id];
        tiles[face1.id] = t;
        tiles[face2.id] = s;
        face1.PutTile(t);
        face2.PutTile(s);
    }
}