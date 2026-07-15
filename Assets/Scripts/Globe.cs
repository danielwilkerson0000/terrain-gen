using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Globe : MonoBehaviour
{
    [HideInInspector]
    List<Face> faces;
    Dictionary<Face, Tile> tiles;
    Globehedron polyhedron;
    GlobeController controller;

    void Awake()
    {
        InitTiles();
        InitController();
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    /// <summary>
    /// Naive search for the closest face to target point.
    /// </summary>
    public Face GetClosestFace(Vector3 target)
    {
        if (faces is null || !faces.Any())
        {
            return null;
        }

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
        Debug.Log($"Closest to {target} is {closest}");

        return closest;
    }

    public float GetFaceScale()
    {
        return polyhedron != null ? polyhedron.scale : 1f;
    }

    void InitTiles()
    {
        if (tiles is not null) return;

        faces = new();
        tiles = new();

        // creates + places faces in the word
        InitGoldbergFaces();


        // creates + places tiles on all faces
        PlaceTiles(0.2f);
    }


    void PlaceTiles(float chance)
    {
        foreach (Face face in faces)
        {
            // Create a new tile and align it to its parent face
            GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tileObject.transform.position = Vector3.zero;
            tileObject.transform.SetParent(face.transform, false);
            tileObject.transform.localScale = Vector3.one * polyhedron.scale;

            Tile tile = tileObject.AddComponent<Tile>();
            tile.Place(face);
            tiles.Add(face, tile);

            // give a few some color!
            if (Random.value < chance)
            {
                Renderer r = tileObject.GetComponent<Renderer>();
                r.material.color = Random.ColorHSV(0, 1, 0.3f, 0.7f, 0.4f, 0.7f);
            }
        }
    }

    void InitGoldbergFaces()
    {
        polyhedron = transform.GetComponent<Globehedron>();
        polyhedron.Generate(transform);
        faces = polyhedron.faces;

        foreach (Face face in faces)
        {
            face.transform.SetParent(polyhedron.babysitter.transform);
        }
    }

    public void SwapTiles(Face face1, Face face2)
    {
        Tile s = tiles[face1];
        Tile t = tiles[face2];
        tiles[face1] = t;
        tiles[face2] = s;
        face1.PlaceTile(t);
        face2.PlaceTile(s);
    }
}