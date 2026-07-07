using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;

public class SelectableGrid : MonoBehaviour
{
    public LayerMask selectables;

    GameObject selected;
    GameObject dragged;
    bool dragging = false;
    bool selecting = false;

    public int height;
    public int width;
    [HideInInspector]
    public Vector3 origin;
    public Vector3 offset;
    List<Vector3> slots;
    List<GameObject> faces;
    Dictionary<GameObject, Tile> tiles;
    GoldbergPolyhedron polyhedron;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = 0.5f * new Vector3(height - 1, 0, width - 1);
        if (tiles is null)
        {
            InitTiles();
        }
        ;
        selected = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selected.SetActive(false);

        dragged = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dragged.SetActive(false);
    }

    public GameObject GetClosestFace(Vector3 hitPosition)
    {
        float dist = float.MaxValue;
        GameObject closest = faces.Any() ? faces[0] : null;
        foreach (GameObject t in faces)
        {
            Vector3 p = t.transform.position;
            float m = (p - hitPosition).sqrMagnitude;
            if (m < dist)
            {
                dist = m;
                closest = t;
            }
        }

        return closest;
    }

    void InitTiles()
    {
        // slots = new();
        tiles = new();
        faces = new();


        /**
            THESE INIT TILE LOCATIONS AND GAMEOBJECTS
        **/
        // MakeRandomSphericalTiles();        
        InitGoldbergTiles();


        // THESE PLACE RANDOM TILES IN THE WORLD
        FillSpotsWithTiles(0.2f);

    }


    void FillSpotsWithTiles(float chance)
    {
        foreach (GameObject g in faces)
        {
            Tile tile = new();
            tiles.Add(g, tile);
            if (Random.value < chance)
            {
                tile.Color(Random.ColorHSV(0, 1, 0.3f, 0.7f, 0.4f, 0.7f));
            }
        }
    }

    // IGNORE THIS
    void MakeRandomSphericalTiles()

    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 p = 10 * UnityEngine.Random.onUnitSphere;
                // if the random point is close to a previous one
                if (slots.Exists(s => (s - p).sqrMagnitude < 1))
                {
                    Debug.Log("discared!");
                    continue;
                }

                slots.Add(p);

                if (UnityEngine.Random.value < 0.2)
                {
                    // tiles.Add(p, new Tile());
                }


            }
        }
    }

    // IGNORE THIS
    void MakeSphericalTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float phi = Mathf.PI * i / (width - 1);
                float theta = 2 * Mathf.PI * j / height;
                // if (phi == 0 || theta == 0) continue;
                // Vector3 slot = new Vector3(i, 0, j) + offset - origin;
                Vector3 slot = new Vector3(
                    Mathf.Cos(theta) * Mathf.Sin(phi),
                    Mathf.Cos(phi),
                    Mathf.Sin(theta) * Mathf.Sin(phi)
                );

                if (slots.Contains(slot)) continue;
                
                slots.Add(slot);

                if (UnityEngine.Random.value < 0.2)
                {
                    // tiles.Add(slot, new Tile());
                }


            }
        }
    }

    void InitGoldbergTiles()
    {
        polyhedron = transform.GetComponent<GoldbergPolyhedron>();
        polyhedron.Generate(transform);
        faces = polyhedron.tiles;
        // slots = faces.Select(t => t.transform.position).ToList();
        // Debug.Log(slots);

        foreach (GameObject face in faces)
        {
            face.transform.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManageMouseControls();
        FinalizeMouseControls();
    }

    void ManageMouseControls()
    {
        // Try to drag the closest face to hit.
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Perform the physics raycast
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectables))
            {
                dragging = true;
                dragged.transform.position = GetClosestFace(hit.point).transform.position;
            }
        }

        // Try to select the closest face to hit
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Perform the physics raycast
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectables))
            {
                selecting = true;
                // Access data about the object that was struck
                Debug.Log("Selected " + hit.transform.name + " " + hit.transform.position + " at point " + hit.point);

                selected.transform.position = GetClosestFace(hit.point).transform.position;
            }
            else
            {
                selecting = false;
            }
        }

        // If we are dragging and selecting the same face, assume we are actually NOT dragging.
        // For safety <3
        if ((selected.transform.position - dragged.transform.position).sqrMagnitude < 0.01f)
        {
            dragging = false;
        }
    }

    // Finailize dragging and switching Tiles
    void FinalizeMouseControls()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // moves tile S from A to B
            // or switches tile S at A with T at B
            if (dragging && selecting)
            {
                Vector3 u = selected.transform.position;
                GameObject a = GetClosestFace(u);
                Vector3 v = dragged.transform.position;
                GameObject b = GetClosestFace(v);

                if (tiles.ContainsKey(a))
                {
                    Tile s = tiles[a];
                    tiles.Remove(a);
                    if (tiles.ContainsKey(b))
                    {
                        Tile t = tiles[b];
                        tiles.Remove(b);
                        tiles.Add(a, t);
                    }
                    tiles.Add(b, s);
                }
            }
            dragging = false;
            selecting = false;
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && !faces.Any())
        {
            InitTiles();
        }

        float scale = 0.5f * Mathf.Pow(1/2f, polyhedron.frequency);
        Vector3 faceScale = Vector3.one * scale;
        foreach (GameObject g in faces)
        {
            Matrix4x4 orig = Gizmos.matrix;
            Vector3 pos = g.transform.position;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, pos - transform.position);
            Matrix4x4 goal = Matrix4x4.TRS(pos - transform.position, rot, faceScale);
            Gizmos.matrix = goal;

            Gizmos.color = Color.white;
            if (tiles.ContainsKey(g))
            {
                Gizmos.color = tiles[g].color;
                Gizmos.DrawSphere(Vector3.zero, 1f);
            }

            Gizmos.matrix = orig;
            // Gizmos.DrawRay(pos, rot * Vector3.forward * 0.4f);
            // Gizmos.DrawRay(pos, rot * Vector3.up * 0.4f);
        }

        if (selecting)
        {
            Gizmos.color = Color.darkBlue;
            Gizmos.DrawWireCube(selected.transform.position, faceScale);
        }

        if (dragging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(dragged.transform.position, faceScale);
        }
    }
}