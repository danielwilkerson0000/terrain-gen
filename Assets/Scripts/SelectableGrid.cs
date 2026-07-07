using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
    List<GameObject> spots;
    Dictionary<Vector3, Tile> tiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = 0.5f * new Vector3(height - 1, 0, width - 1);
        if (tiles is null)
        {
            InitSlots();
        };
        selected = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selected.SetActive(false);

        dragged = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dragged.SetActive(false);
    }

    public Vector3 GetClosestSlot(Vector3 position)
    {
        
        /*

        getClosestSlot(V3 pos) {
        for (slot t in slots) {
        if slot.id(pos) = grid.id(pos)
        }
        }

        */

        float dist = float.MaxValue;
        Vector3 closest = Vector3.zero;
        foreach (Vector3 slot in slots)
        {
            float m = (slot - position).sqrMagnitude;
            if (m < dist)
            {
                dist = m;
                closest = slot;
            }
        }

        return closest;
    }

    void InitSlots()
    {
        slots = new();
        tiles = new();
        spots = new();

        // MakeRandomSphericalTiles();        
        MakeGoldbergTiles();
    }
    
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
                    tiles.Add(p, new Tile());
                }


            }
        }
    }

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
                    tiles.Add(slot, new Tile());
                }


            }
        }
    }

    void MakeGoldbergTiles()
    {
        GoldbergPolyhedron p = transform.GetComponent<GoldbergPolyhedron>();
        p.Generate(transform);
        slots = p.tiles.Select(t => t.transform.position).ToList();
        spots = p.tiles;
        // Debug.Log(slots);

        foreach (GameObject tile in p.tiles)
        {
            tile.transform.SetParent(transform);
        }
        foreach (GameObject g in spots)
        {
            if (Random.value < 0.2f)
            {
                tiles.Add(g.transform.position, new Tile());
            }
        }
        // foreach (Vector3 slot in slots)
        // {
        //     if (Random.value < 0.2f)
        //     {
        //         tiles.Add(slot, new Tile());
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Perform the physics raycast
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectables))
            {
                dragging = true;
                dragged.transform.position = GetClosestSlot(hit.point);
            }
        }
        // Check if the left mouse button is clicked
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

                selected.transform.position = GetClosestSlot(hit.point);
            }
            else
            {
                selecting = false;
            }
        }
       

        if ((selected.transform.position - dragged.transform.position).sqrMagnitude < 0.01f)
        {
            dragging = false;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {

            // moves tile S from A to B
            // or switches tile S at A with T at B
            if (dragging && selecting)
            {
                Vector3 a = selected.transform.position;
                Vector3 b = dragged.transform.position;
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
        if (Application.isPlaying && !spots.Any())
        {
            InitSlots();
        }

        // foreach (Vector3 slot in slots)
        
        foreach (GameObject g in spots)
        {
            Vector3 pos = g.transform.position;
            Matrix4x4 m = transform.localToWorldMatrix;

            Matrix4x4 orig = Gizmos.matrix;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, pos - transform.position);
            Matrix4x4 goal = Matrix4x4.TRS(pos - transform.position, rot, Vector3.one);
            Gizmos.matrix = goal;

            Gizmos.color = Color.white;
            Gizmos.DrawCube(Vector3.zero, 0.25f * Vector3.one);

            if (tiles.ContainsKey(pos))
            {
                Gizmos.color = tiles[pos].color;
                Gizmos.DrawCube(Vector3.zero, 0.3f * Vector3.one);
            }

            Gizmos.matrix = orig;

            Gizmos.DrawRay(pos, rot * Vector3.forward * 0.4f);
            Gizmos.DrawRay(pos, rot * Vector3.up * 0.4f);
        }

        if (selecting)
        {
            Gizmos.color = Color.darkBlue;
            Gizmos.DrawWireCube(selected.transform.position, 0.4f * Vector3.one);
        }

        if (dragging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(dragged.transform.position, 0.4f * Vector3.one);
        }
    }
}